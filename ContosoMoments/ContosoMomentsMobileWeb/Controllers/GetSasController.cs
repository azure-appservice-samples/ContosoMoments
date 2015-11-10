using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContosoMoments.Common.Srorage;

namespace ContosoMoments.MobileServices.Controllers
{


    public class GetSasController : ApiController
    {
        // GET api/getsasurl
        public IEnumerable<string> Get()
        {
           

            var storageCorsHelper = new ContosoStorage();
            var result = storageCorsHelper.GetSasUrlAndSetCORS("Upload", Guid.NewGuid().ToString());
        


            return new string[] { result };

        }

    }
}
