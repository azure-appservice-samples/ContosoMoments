using ContosoMoments.Common;
using ContosoMoments.Common.Queue;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class CommitBlobController : ApiController
    {
        public async Task Post(JObject body)
        {
            string imageId = body["imageId"].ToString();
            string extension = body["extension"].ToString();
            var qm = new QueueManager();
            var blobInfo = new BlobInformation(extension);
            blobInfo.BlobUri = new Uri(string.Format("https://{0}.blob.core.windows.net", AppSettings.StorageAccountName));
            blobInfo.ImageId = imageId;

            await qm.PushToResizeQueue(blobInfo);
            Trace.WriteLine("Sent resize request for blob: " + imageId);
        }
    }
}

