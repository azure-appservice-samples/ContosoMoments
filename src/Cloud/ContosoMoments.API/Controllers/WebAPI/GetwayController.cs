using System.Web.Http;
using ContosoMoments.Common;
using Microsoft.Azure.Mobile.Server.Config;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
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
