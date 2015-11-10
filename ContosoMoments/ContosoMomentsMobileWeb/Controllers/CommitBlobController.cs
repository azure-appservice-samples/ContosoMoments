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
    public class CommitBlobController : ApiController
    {
        public ApiServices Services { get; set; }

        // POST: api/Default
        public bool Post([FromBody]CommitBlobRequest commitBlobRequest)
        {
            var cs = new ContosoStorage();
            return cs.CommitUpload(commitBlobRequest);

        }

    }
}
