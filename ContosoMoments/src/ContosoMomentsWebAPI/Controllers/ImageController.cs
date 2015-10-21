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
                    //TODO: Log database not exists
                    return null;
                }
            }
            catch (Exception ex)
            {
                //TODO: Log exception
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
                    //TODO: Log database not exists
                    return null;
                }
            }
            catch (Exception ex)
            {
                //TODO: Log exception
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
                                //TODO: Log DB update failed
                                await RemoveImageFromBlob(imageId, account);
                                return false;
                            }
                        }
                        else
                        {
                            //TODO: Log blob was not created
                            return false;
                        }
                    }
                    else
                    {
                        //TODO: Log not supported image type or format
                        return false;
                    }
                }
                else
                {
                    //TODO: Log multiple images uploaded or wrong content type images
                    return false;
                }
            }

            //TODO: Log request has no image in the body
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
                    //TODO: Log DB not exists...
                }

            }
            catch (Exception ex)
            {
                //LOG db exception
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
                //TODO: Log queue exception
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
                //TODO: Log queue exception
            }
        }

        private async Task<Uri> UplodImageToBlob(object imageId, CloudStorageAccount account, IFormFile formFile)
        {
            try
            {
                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer imagesContainer = client.GetContainerReference(_appSettings.Options.BaseContainer + "/" + _appSettings.Options.LargeImages);
                CloudBlockBlob blob = imagesContainer.GetBlockBlobReference(imageId.ToString() + FILE_EXT);
                blob.Properties.ContentType = formFile.ContentType;
                await blob.UploadFromStreamAsync(formFile.OpenReadStream(), formFile.Length);

                return client.GetContainerReference(_appSettings.Options.BaseContainer).Uri;
            }
            catch (Exception ex)
            {
                //LOG storage exception + ex
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
                    //TODO: Log DB not exists...
                    return false;
                }

            }
            catch (Exception ex)
            {
                //LOG db exception
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
                //LOG storage exception + ex
            }
        }
        #endregion
    }
}
