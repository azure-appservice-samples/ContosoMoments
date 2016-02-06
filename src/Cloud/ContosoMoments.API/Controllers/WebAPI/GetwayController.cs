using Microsoft.Azure.Mobile.Server.Config;
using System.Web.Http;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class GetwayController : ApiController
    {
        // GET api/Getway
        public string Get()
        {
            return "pong";
        }
    }
}