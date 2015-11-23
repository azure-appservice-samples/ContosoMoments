using Microsoft.Azure.Mobile.Server.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class LikeController : ApiController
    {
        // POST: api/Like
        public void Post([FromBody]string imageId)
        {
            // 
        }
    }
}
