using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.NotificationHubs;

namespace ContosoMoments.Common.Models
{
    public class DeviceRegistration : EntityData
    {
        public string InstallationId { get; set; }
        public string UserId { get; set; }
        public NotificationPlatform Platform { get; set; }
    }
}
