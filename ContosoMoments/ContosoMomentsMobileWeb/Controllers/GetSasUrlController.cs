using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContosoMoments.Common.Srorage;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.MobileServices.Controllers
{
    public class GetSasUrlController : ApiController
    {
        public ApiServices Services { get; set; }

       

        // GET api/getsasurl
        public string Get()
        {
            Services.Log.Info("Start - GetSasUrlController ");
            var containerName = "uploadFolder";
            var fileName = Guid.NewGuid().ToString();
            var cs = new ContosoStorage();
            var result = cs.GetSasUrlAndSetCORS(containerName, fileName);
            Services.Log.Info("End - GetSasUrlController ");


            return   result ;

        }

    }
}
