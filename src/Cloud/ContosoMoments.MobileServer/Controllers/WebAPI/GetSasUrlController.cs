using System;
using System.Web.Http;
using System.Web.Http.Cors;
using ContosoMoments.Common;
using ContosoMoments.Common.Storage;
using Microsoft.Azure.Mobile.Server.Config;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class GetSasUrlController : ApiController
    {

        [EnableCors(origins: "*", headers: "*", methods: "*")]
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

