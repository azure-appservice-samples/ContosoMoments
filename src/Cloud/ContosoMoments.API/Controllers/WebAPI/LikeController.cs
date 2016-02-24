using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ContosoMoments.Common.Models;
using ContosoMoments.Common;

namespace ContosoMoments.Api
{
    [MobileAppController]
    public class LikeController : ApiController
    {
        // POST: api/Like
        public async Task<bool> Post([FromBody]Dictionary<string, string> imageInfo)
        {
            using (var ctx = new MobileServiceContext()) {
                var image = ctx.Images.Include("User").SingleOrDefault(x => x.Id == imageInfo["imageId"]);

                if (image != null) {
                    var registrations = ctx.DeviceRegistrations.Where(x => x.UserId == image.UserId);

                    //Send plat-specific message to all installation one by one
                    string message = "Someone has liked your image";
                    foreach (var registration in registrations) {
                        await SendPush(message, registration);
                    }

                    return true;
                }

                return false;
            }
        }

        private static async Task SendPush(string message, DeviceRegistration registration)
        {
            var tags = new string[1] { $"$InstallationId:{{{registration.InstallationId}}}" };
            switch (registration.Platform) {
                case NotificationPlatform.Wns:
                    await Notifier.Instance.SendWindowsNotification(message, tags);
                    break;
                case NotificationPlatform.Apns:
                    await Notifier.Instance.SendAppleNotification(message, tags);
                    break;
                case NotificationPlatform.Mpns:
                    await Notifier.Instance.SendMpnsNotification(message, tags);
                    break;
                case NotificationPlatform.Gcm:
                    await Notifier.Instance.SendGcmNotification(message, tags);
                    break;
                case NotificationPlatform.Adm:
                    //NOT SUPPORTED
                    break;
                default:
                    break;
            }
        }
    }
}