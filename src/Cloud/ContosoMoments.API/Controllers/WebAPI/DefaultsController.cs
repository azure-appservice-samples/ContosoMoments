using Microsoft.Azure.Mobile.Server.Config;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using ContosoMoments.Common;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class DefaultsController : ApiController
    {
        public JObject GetDefaults()
        {
            var result = new JObject();
            result["DefaultUserId"]  = AppSettings.DefaultUserId;
            result["DefaultAlbumId"] = AppSettings.DefaultAlbumId;

            return result;
        }
    }
}