using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ContosoMoments.API.Helpers;
using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;

namespace ContosoMoments.API.Controllers.TableControllers
{
    public class ImageStorageController : StorageController<Image>
    {
        public ImageStorageController() : 
            base(new CustomAzureStorageProvider(GetConnectionString()))
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

        //[HttpDelete]
        //[Route("tables/Image/{id}/MobileServiceFiles/{name}")]
        //public Task Delete(string id, string name)
        //{
        //    return base.DeleteFileAsync(id, name, new ImageNameResolver());
        //}

        private static string GetConnectionString(string connectionStringName = Constants.StorageConnectionStringName)
        {
            if (connectionStringName == null) {
                throw new ArgumentNullException("connectionStringName");
            }

            if (connectionStringName.Length == 0) {
                throw new ArgumentException("Connection string should not be empty");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionStringSettings == null) {
                throw new ConfigurationErrorsException(string.Format("Connection string is missing", connectionStringName));
            }

            return connectionStringSettings.ConnectionString;
        }
    }
}
