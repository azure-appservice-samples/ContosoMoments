using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoMoments.Api
{
    public class ImageStorageController : StorageController<Image>
    {
        public ImageStorageController() : 
            base(new CustomAzureStorageProvider())
        { }

        [HttpPost]
        [Route("tables/Image/{id}/StorageToken")]
        // return a storage token that can be used for blob upload or download
        public async Task<HttpResponseMessage> PostStorageTokenRequest(string id, StorageTokenRequest request)
        {
            // The file size is encoded in the start of the filename, lg, md, etc.
            // If it doesn't match the pattern then the default size of lg is used
            var requestedSize = request.TargetFile.Name.Substring(0, 2); 

            StorageToken token = await GetStorageTokenAsync(id, request, new ImageNameResolver(requestedSize));
            return Request.CreateResponse(token);
        }

        /// Get the files associated with this record
        [HttpGet]
        [Route("tables/Image/{id}/MobileServiceFiles")]
        public async Task<HttpResponseMessage> GetFiles(string id)
        {
            var files = await GetRecordFilesAsync(id, new ImageNameResolver());      

            return Request.CreateResponse(files);
        }

        // there's no Delete method, because deletion is handled by deleting the image itself
    }
}
