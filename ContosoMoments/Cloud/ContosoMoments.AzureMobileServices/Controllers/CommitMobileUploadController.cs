using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContosoMoments.Common.Srorage;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ContosoMoments.AzureMobileServices.Controllers
{
    public class CommitMobileUploadController : ApiController
    {
        public ApiServices Services { get; set; }

        // Post api/CommitMobileUpload
        public bool Post([FromBody]CommitBlobRequest commitBlobRequest)
        {
            var cs = new ContosoStorage();
            return cs.CommitUpload(commitBlobRequest);

        }
    }
}
