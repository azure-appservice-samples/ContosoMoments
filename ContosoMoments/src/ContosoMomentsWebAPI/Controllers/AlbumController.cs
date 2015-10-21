using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ContosoMomentsCommon.Models;
using Microsoft.Framework.OptionsModel;
using ContosoMomentsWebAPI.Model;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using ContosoMomentsCommon;
using Newtonsoft.Json;


// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class AlbumController : Controller
    {
        #region Consts and variables
        private IOptions<AppSettings> _appSettings { get; set; }
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region .ctor
        public AlbumController(IOptions<AppSettings> appSettings, IServiceProvider serviceProvider)
        {
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
        }
        #endregion

        #region Web APIs
        // GET: api/album
        [HttpGet]
        public IEnumerable<Album> Get()
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    return context.Albums.ToList();
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

        // GET api/album/5
        [HttpGet("{id}")]
        public IEnumerable<Image> Get(Guid id)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    IQueryable<Image> images = context.Images.Where(p => p.AlbumId == id);

                    return images.ToList();
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

        // POST api/album/test_album
        [HttpPost]
        public async Task<bool> Post([FromBody]string AlbumName)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    var album = new Album() { AlbumId = Guid.NewGuid(), AlbumName = AlbumName };
                    context.Albums.Add(album);
                    await context.SaveChangesAsync();

                    return true;
                }
                else
                {
                    //TODO: Log database not exists
                    return false;
                }
            }
            catch (Exception ex)
            {
                //TODO: Log exception
                return false;
            }
        }

        // PUT api/album/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]Album metadata)
        {
            //UNSUPPORTED for P1
        }

        // DELETE api/album/5
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    IQueryable<Image> images = context.Images.Where(p => p.AlbumId == id);

                    if (images.Count() > 0)
                    {
                        //Queue removal of all blobs
                        await QueueDeleteRequests(images.ToList());
                        //Delete records from the DB
                        await DeleteImagesFromDB(images.ToArray());
                    }
                    //Delete album from the DB
                    await DeleteAlbumFromDB(id);
                }
                else
                {
                    //TODO: Log database not exists
                }
            }
            catch (Exception ex)
            {
                //TODO: Log exception
            }
        }
        #endregion

        #region Private functionality
        private async Task QueueDeleteRequests(List<Image> images)
        {
            try
            {
                CloudStorageAccount account;
                if (CloudStorageAccount.TryParse(_appSettings.Options.StorageConnectionString, out account))
                {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue resizeRequestQueue = queueClient.GetQueueReference("deleterequest");
                    resizeRequestQueue.CreateIfNotExists(); //Make sure the queue exists

                    foreach (var image in images)
                    {
                        BlobInformation blobInfo = new BlobInformation() { ImageId = image.ToString(), BlobUri = new Uri(image.ContainerName) };
                        var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                        await resizeRequestQueue.AddMessageAsync(queueMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                //LOG queue exception
            }
        }

        private async Task DeleteImagesFromDB(Image[] images)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    context.Images.RemoveRange(images);
                    await context.SaveChangesAsync();
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
        }

        private async Task DeleteAlbumFromDB(Guid albumId)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    IQueryable<Album> album = context.Albums.Where(p => p.AlbumId == albumId);
                    if (album.Count() > 0)
                    {
                        context.Albums.Remove(album.First());
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
        }
        #endregion
    }
}
