using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using ContosoMomentsWebAPI.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using ContosoMomentsCommon;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class LikeController : Controller
    {
        #region Consts and variables
        private IOptions<AppSettings> _appSettings { get; set; }
        #endregion

        #region .ctor
        public LikeController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }
        #endregion

        #region Web APIs
        // POST api/like { imageId }
        [HttpPost]
        public async Task Post([FromBody]Guid ImageId)
        {
            await QueueDeleteRequests(ImageId);
        }
        #endregion

        #region Private functionality
        private async Task QueueDeleteRequests(Guid imageId)
        {
            try
            {
                CloudStorageAccount account;
                if (CloudStorageAccount.TryParse(_appSettings.Options.StorageConnectionString, out account))
                {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue resizeRequestQueue = queueClient.GetQueueReference("pushnotificationrequest");
                    resizeRequestQueue.CreateIfNotExists(); //Make sure the queue exists

                    BlobInformation blobInfo = new BlobInformation() { ImageId = imageId.ToString() };
                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                    await resizeRequestQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex)
            {
                //LOG queue exception
            }
        }
        #endregion

    }
}
