using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using ContosoMomentsWebAPI.Model;
using ContosoMomentsCommon.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using ContosoMomentsCommon;
using Newtonsoft.Json;
using Microsoft.AspNet.Http;
using Microsoft.Data.Entity;
using Microsoft.Framework.DependencyInjection;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class ImageController : Controller
    {
        #region Consts and variables
        public const string SUPPORTED_CONTENT_TYPE = "image/jpeg";
        public const string FILE_EXT = ".jpg";

        private IOptions<AppSettings> _appSettings { get; set; }
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region .ctor
        public ImageController(IOptions<AppSettings> appSettings, IServiceProvider serviceProvider)
        {
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
        }
        #endregion

        #region Web APIs
        // GET: api/image?page=5
        [HttpGet]
        public IEnumerable<Image> Get([FromQuery] int? page)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    IOrderedQueryable<Image> images = context.Images.OrderBy(p => p.ImageId);

                    if (!page.HasValue)
                        page = 0;
                    else
                        page -= 1;

                    int pageSize = int.Parse(_appSettings.Options.PageSize);

                    IQueryable<Image> imagesOnPage = images.Skip(page.Value * pageSize).Take(pageSize);

                    return imagesOnPage.ToList();
                }
                else
                {
                    Trace.TraceWarning("[GET] /api/image/: Database not exists");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in [GET] /api/image/ => " + ex.Message);
                return null;
            }

        }

        // GET: api/image/3
        [HttpGet("{id}")]
        public Image Get(Guid id)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    IQueryable<Image> image = context.Images.Where(p => p.ImageId == id);

                    return image.FirstOrDefault();
                }
                else
                {
                    Trace.TraceWarning("[GET] /api/image/{id}: Database not exists");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in [GET] /api/image/{id} => " + ex.Message);
                return null;
            }
        }

        // POST api/image?User=000-0000-0000..&Album=111-111-111...
        [HttpPost]
        public async Task<bool> Post([FromQuery]string User, [FromQuery]string Album)
        {
            Guid imageId = Guid.NewGuid();

            //Check that request has form content with single JPEG file attached
            if (Request.HasFormContentType)
            {
                var form = Request.Form;
                if (form.Files.Count == 1 && form.Files[0].ContentType == SUPPORTED_CONTENT_TYPE)
                {
                    //Connect to the storage account an upload to "Large images" container
                    CloudStorageAccount account;
                    if (CloudStorageAccount.TryParse(_appSettings.Options.StorageConnectionString, out account))
                    {
                        //Save uploaded image to the blob
                        Uri containerUri = await UplodImageToBlob(imageId, account, form.Files[0]);

                        if (null != containerUri)
                        {
                            //Update DB with new imageId, container Uri, and username/album
                            bool res = await UpdateDB(imageId, User, Album, containerUri);

                            if (res)
                            {
                                //Finally, add new queue message to start resizing WebJob
                                await QueueResizeRequest(imageId, account, containerUri);
                                return true;
                            }
                            else
                            {
                                Trace.TraceWarning("[POST] /api/image/: Database update failed, removing uploaded image from blob");
                                await RemoveImageFromBlob(imageId, account);
                                return false;
                            }
                        }
                        else
                        {
                            Trace.TraceWarning("[POST] /api/image/: Blob update failed");
                            return false;
                        }
                    }
                    else
                    {
                        Trace.TraceWarning("[POST] /api/image/: Storage account connection failed");
                        return false;
                    }
                }
                else
                {
                    Trace.TraceWarning("[POST] /api/image/: Not supported image format or multiple images uploaded");
                    return false;
                }
            }

            Trace.TraceWarning("[POST] /api/image/: Request has no image in the FormBody");
            return false;
        }

        // DELETE api/image/5
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            Uri ContainerUri = await DeleteFromDB(id);
            await QueueDeleteRequest(id, ContainerUri);
        }
        #endregion

        #region Private functionality
        private async Task<Uri> DeleteFromDB(Guid imageId)
        {
            Uri retVal = null;

            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    IQueryable<Image> image = context.Images.Where(p => p.ImageId == imageId);
                    if (image.Count() > 0)
                    {
                        retVal = new Uri(image.First().ContainerName);
                        context.Images.Remove(image.First());
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    Trace.TraceWarning("ImageController.DeleteFromDB: Database not exists");
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in ImageController.DeleteFromDB => " + ex.Message);
            }

            return retVal;
        }

        private async Task QueueDeleteRequest(Guid imageId, Uri ContainerUri)
        {
            try
            {
                CloudStorageAccount account;
                if (CloudStorageAccount.TryParse(_appSettings.Options.StorageConnectionString, out account))
                {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue resizeRequestQueue = queueClient.GetQueueReference("deleterequest");
                    resizeRequestQueue.CreateIfNotExists(); //Make sure the queue exists

                    BlobInformation blobInfo = new BlobInformation() { ImageId = imageId.ToString(), BlobUri = ContainerUri };
                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                    await resizeRequestQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in ImageController.QueueDeleteRequest => " + ex.Message);
            }
        }

        private async Task QueueResizeRequest(Guid imageId, CloudStorageAccount account, Uri blobUri)
        {
            try
            {
                CloudQueueClient queueClient = account.CreateCloudQueueClient();
                CloudQueue resizeRequestQueue = queueClient.GetQueueReference("resizerequest");
                resizeRequestQueue.CreateIfNotExists(); //Make sure the queue exists

                BlobInformation blobInfo = new BlobInformation() { ImageId = imageId.ToString(), BlobUri = blobUri };
                var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                await resizeRequestQueue.AddMessageAsync(queueMessage);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in ImageController.QueueResizeRequest => " + ex.Message);
            }
        }

        private async Task<Uri> UplodImageToBlob(object imageId, CloudStorageAccount account, IFormFile formFile)
        {
            try
            {
                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer imagesContainer = client.GetContainerReference(_appSettings.Options.BaseContainer + "/" + _appSettings.Options.LargeImages);
                if (client.GetContainerReference(_appSettings.Options.BaseContainer).GetPermissions().PublicAccess != BlobContainerPublicAccessType.Container) //Check and fix permissions for new container
                    imagesContainer.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Container });
                CloudBlockBlob blob = imagesContainer.GetBlockBlobReference(imageId.ToString() + FILE_EXT);
                blob.Properties.ContentType = formFile.ContentType;
                System.IO.Stream stream = formFile.OpenReadStream();
                if (null != stream)
                {
                    await blob.UploadFromStreamAsync(stream, stream.Length);

                    return client.GetContainerReference(_appSettings.Options.BaseContainer).Uri;
                }
                else
                {
                    throw new Exception("Cannot open incoming file stream");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in ImageController.UplodImageToBlob => " + ex.Message);
                return null;
            }
        }

        private async Task<bool> UpdateDB(Guid imageId, string userId, string albumId, Uri containerUri)
        {
            Guid uId, aId;
            if (null == userId)
                uId = new Guid(_appSettings.Options.DefaultId);
            else
                uId = Guid.Parse(userId);

            if (null == albumId)
                aId = new Guid(_appSettings.Options.DefaultId);
            else
                aId = Guid.Parse(albumId);

            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    Image newImage = new Image() { ImageId = imageId, ContainerName = containerUri.ToString(), AlbumId = uId, UserId = uId, ImageFormat = "image/jpeg" };
                    context.Images.Add(newImage);
                    await context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    Trace.TraceWarning("ImageController.UpdateDB: Database not exists");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in ImageController.UpdateDB => " + ex.Message);
                return false;
            }
        }

        private async Task RemoveImageFromBlob(Guid imageId, CloudStorageAccount account)
        {
            try
            {
                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer imagesContainer = client.GetContainerReference(_appSettings.Options.BaseContainer + "/" + _appSettings.Options.LargeImages);
                CloudBlockBlob blob = imagesContainer.GetBlockBlobReference(imageId.ToString() + FILE_EXT);
                await blob.DeleteAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in ImageController.RemoveImageFromBlob => " + ex.Message);
            }
        }
        #endregion
    }
}
