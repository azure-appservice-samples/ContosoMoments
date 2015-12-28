using ContosoMoments.Common.Notification;
using ContosoMoments.MobileServer.DataLogic;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class LikeController : ApiController
    {
        // POST: api/Like
        public async Task<bool> Post([FromBody]Dictionary<string,string> image)
        {
            var logic = new ImageBusinessLogic();

            var img = logic.GetImage(image["imageId"]);
            if (img!=null && img.User.Email != User.Identity.Name )
            {
                var message = new Dictionary<string, string>()
                {
                    { "message",string.Format("{0} has liked your image",User.Identity.Name)}
                };
                var tags = new string[1] { img.User.Email };
                //var tags = new string[1] { "uemail:noynir@gmail.com" };
                var res = await Notifier.Instance.SendTemplateNotification(message, tags);
                return res;
            }

            return false;

     

        }
    }
}
