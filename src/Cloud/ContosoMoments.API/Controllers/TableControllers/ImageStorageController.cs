using ContosoMoments.API.Helpers;
using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;

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
            StorageToken token = await GetStorageTokenAsync(id, request, new ImageNameResolver(request.TargetFile.StoreUri));
            return Request.CreateResponse(token);
        }

        /// Get the files associated with this record
        [HttpGet]
        [Route("tables/Image/{id}/MobileServiceFiles")]
        public async Task<HttpResponseMessage> GetFiles(string id)
        {
            IEnumerable<MobileServiceFile> files = await GetRecordFilesAsync(id, new ImageNameResolver());
            return Request.CreateResponse(files);
        }

        [HttpDelete]
        [Route("tables/Image/{id}/MobileServiceFiles/{name}")]
        [Authorize]
        public Task Delete(string id, string name)
        {
            return base.DeleteFileAsync(id, name, new ImageNameResolver());
        }
    }
}
