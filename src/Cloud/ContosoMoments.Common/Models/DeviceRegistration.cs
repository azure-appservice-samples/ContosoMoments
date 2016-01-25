using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments.Common.Models
{
    public class DeviceRegistration : EntityData
    {
        public string InstallationId { get; set; }
        public string UserId { get; set; }
        public NotificationPlatform Platform { get; set; }
    }
}
