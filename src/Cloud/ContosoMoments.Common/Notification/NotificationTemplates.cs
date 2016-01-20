using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace ContosoMoments.Common.Notification
{
    public class NotificationTemplates
    {
        
        public string WindowsPhoneTemplate { get; set; }
        public string AndroidTemplate { get; set; }
        public string IOSTemplate { get; set; }

        public NotificationTemplates(string messgae)
        {
            AndroidTemplate = "{\"data\":{\"msg\":\"" + messgae + "\"}}";

            WindowsPhoneTemplate = "";

            IOSTemplate = "";
        }
    }
}
