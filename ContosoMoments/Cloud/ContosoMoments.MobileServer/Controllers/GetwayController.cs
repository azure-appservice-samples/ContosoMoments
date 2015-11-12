using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContosoMoments.Common;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;

namespace ContosoMoments.MobileServer.Controllers
{
    [MobileAppController]
    public class GetwayController : ApiController
    {
        

        // GET api/Getway
        public string Get()
        {

            return AppSettings.FacebookAuthString;
        }

    }
}
