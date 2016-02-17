using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using ContosoMoments.Common;
using ContosoMoments.Common.Queue;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.API.Controllers.TableController
{
    public class ResizeRequest : EntityData
    {
        public string BlobName { get; set; }
    }

    public class ResizeRequestController : TableController<ResizeRequest>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }

        public async Task<IHttpActionResult> PostRequest(ResizeRequest item)
        {
            item.Deleted = true; // mark as deleted so that the client removes it from the local store
            await PostToQueue(item.BlobName);

            return CreatedAtRoute("Tables", new { id = item.Id }, item);
        }

        public IEnumerable<ResizeRequest> GetAll()
        {
            return Enumerable.Empty<ResizeRequest>();
        }

        private async Task PostToQueue(string blobName)
        {
            var fileNameParts = blobName.Split('.');
            string imageId = fileNameParts[0];
            string extension = fileNameParts.Length > 1 ? fileNameParts[1] : "";

            var qm = new QueueManager();
            var blobInfo = new BlobInformation(extension);
            blobInfo.BlobUri = new Uri(string.Format("https://{0}.blob.core.windows.net", AppSettings.StorageAccountName));
            blobInfo.ImageId = imageId;

            await qm.PushToResizeQueue(blobInfo);
            Trace.WriteLine("Sent resize request for blob: " + imageId);
        }
    }
}
