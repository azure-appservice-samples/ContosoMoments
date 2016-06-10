using ContosoMoments.Common;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;
using System.Web.Http;

namespace ContosoMoments.Api
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