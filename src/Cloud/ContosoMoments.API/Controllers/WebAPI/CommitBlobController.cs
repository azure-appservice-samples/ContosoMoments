using ContosoMoments.Common;
using ContosoMoments.Common.Queue;
using Microsoft.Azure.Mobile.Server.Config;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class CommitBlobController : ApiController
    {
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task Post(JObject body)
        {
            string imageId = body["imageId"].ToString();
            string extension = body["extension"].ToString();
            var qm = new QueueManager();
            var blobInfo = new BlobInformation(extension);
            blobInfo.BlobUri = new Uri(string.Format("https://{0}.blob.core.windows.net", AppSettings.StorageAccountName));
            blobInfo.ImageId = imageId;

            await qm.PushToResizeQueue(blobInfo);
            Debug.WriteLine("Sent resize request for blob: " + imageId);
        }
    }
}

