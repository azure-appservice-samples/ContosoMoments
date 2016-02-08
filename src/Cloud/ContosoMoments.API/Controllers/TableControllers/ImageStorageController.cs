using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;

namespace ContosoMoments.API.Controllers.TableControllers
{
    public class ImageStorageController : StorageController<Image>
    {
        [HttpPost]
        [Route("tables/Image/{id}/StorageToken")]
        public async Task<HttpResponseMessage> PostStorageTokenRequest(string id, StorageTokenRequest value)
        {
            // return a storage token that can be used for blob upload or download
            StorageToken token = await GetStorageTokenAsync(id, value);
            return Request.CreateResponse(token);
        }

        /// Get the files associated with this record
        [HttpGet]
        [Route("tables/Image/{id}/MobileServiceFiles")]
        public async Task<HttpResponseMessage> GetFiles(string id)
        {
            IEnumerable<MobileServiceFile> files = await GetRecordFilesAsync(id);
            return Request.CreateResponse(files);
        }

        [HttpDelete]
        [Route("tables/Image/{id}/MobileServiceFiles/{name}")]
        public Task Delete(string id, string name)
        {
            return base.DeleteFileAsync(id, name);
        }
    }
}
