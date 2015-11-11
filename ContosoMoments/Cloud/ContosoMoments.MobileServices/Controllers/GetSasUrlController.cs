using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using ContosoMoments.Common.Srorage;

namespace ContosoMoments.MobileServices.Controllers
{
    public class GetSasUrlController : ApiController
    {


        [EnableCors(origins: "*", headers: "*", methods: "*")]
        // GET api/getsasurl
        public string Get()
        {
            
            var containerName = "uploadFolder";
            var fileName = Guid.NewGuid().ToString() + ".jpg";
            var cs = new ContosoStorage();
            var result = cs.GetSasUrlAndSetCORS(containerName, fileName);
           


            return   result ;

        }

    }
}

