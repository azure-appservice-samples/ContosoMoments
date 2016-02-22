using ContosoMoments.Common;
using ContosoMoments.Common.Storage;
using Microsoft.Azure.Mobile.Server.Config;
using System;
using System.Web.Http;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class GetSasUrlController : ApiController
    {
        // GET api/getsasurl
        public string Get()
        {
            var fileName = "lg/" +Guid.NewGuid() + ".temp";
            var cs = new ContosoStorage();
            var result = cs.GetSasUrlAndSetCORS(AppSettings.UploadContainerName, fileName);
            return   result ;
        }

    }
}

