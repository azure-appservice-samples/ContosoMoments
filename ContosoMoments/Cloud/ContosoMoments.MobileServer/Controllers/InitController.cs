using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContosoMoments.MobileServer.DataLogic;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.MobileServer.Controllers
{
    public class InitController : ApiController
    {

        // GET api/Init
        public void Get()
        {
            //todo: get the email from easy auth API 
            var init = new InitLogic();
            init.initializeFirstLogin("email from facebook / AD");
        }

    }
}
