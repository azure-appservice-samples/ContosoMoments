using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ContosoMoments.Common.Models;
using ContosoMoments.Common.Srorage;
using Microsoft.Azure.Mobile.Server;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ContosoMoments.MobileServices.Controllers
{
    public class UploadController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET SASUrl
        public string Get(string containerName, string fileName, string fileType )
        {
            Services.Log.Info("Hello from custom controller!");
            var ur = new UploadRequest
            {
                ContainerName = containerName,
                FileName = fileName,
                FileType = fileType
            };
            var su = new StorageUpload();
            var result = su.GetUploadSasUrl(ur);

            return result;
        }

        // Commit Upload 
        public bool Post(CommitBlobRequest commitBlobRequest)
        {
            Services.Log.Info("Hello from custom controller!");
            var su = new StorageUpload();
            var commited = su.CommitUpload(commitBlobRequest);
            return commited;
        }

       
    }
}
