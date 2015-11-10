using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using ContosoMoments.Common.Srorage;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ContosoMoments.MobileServices.Controllers
{
    public class GetSasUrlController : ApiController
    {
        public ApiServices Services { get; set; }


        [EnableCors(origins: "*", headers: "*", methods: "*")]
        // GET api/getsasurl
        public string Get()
        {
            Services.Log.Info("Start - GetSasUrlController ");
            var containerName = "uploadFolder";
            var fileName = Guid.NewGuid().ToString() + ".jpg";
            var cs = new ContosoStorage();
            var result = cs.GetSasUrlAndSetCORS(containerName, fileName);
            Services.Log.Info("End - GetSasUrlController ");


            return   result ;

        }

    }
}

