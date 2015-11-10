using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Services.Protocols;
using ContosoMoments.AzureMobileServices.Common;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ContosoMoments.AzureMobileServices.Controllers
{
    public class UploadFIleController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/UploadFIle
        private static readonly string ServerUploadFolder = "C:\\Temp"; //Path.GetTempPath();

        [Route("files")]
        [HttpPost]
        [ValidateMimeMultipartContentFilter]
        public  bool UploadSingleFile()
        {
            var result = false;
            try
            {
                var streamProvider = new MultipartFormDataStreamProvider(ServerUploadFolder);
                var data = Request.Content.ReadAsMultipartAsync(streamProvider);
                result = true;
            }
            catch (Exception ex)
            {
                
                ///throw ;
            }
         

            return result;
        }

    }
}
