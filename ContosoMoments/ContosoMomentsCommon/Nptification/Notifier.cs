using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Notifications;

namespace ContosoMoments.Common.Nptification
{
    public class Notifier
    {
        public static async Task sendGCMNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
        {
            try
            {
                Trace.TraceInformation("Sending Google notification toast to RegistrationId " + registration.RegistrationId);
                // Define an Android notification.
                var notification = "{\"data\":{\"msg\":\"" + messgae + "\"}}";
                await hub.SendGcmNativeNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Google notification: " + ex.Message);
            }
        }

        public static async Task sendIOSNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
        {
            try
            {
                Trace.TraceInformation("Sending iOS alert to RegistrationId " + registration.RegistrationId);
                // Define an iOS alert.
                var alert = "{\"aps\":{\"alert\":\"" + messgae + "\"}}";
                await hub.SendAppleNativeNotificationAsync(alert);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending iOS alert: " + ex.Message);
            }
        }

        public static async Task sendWPNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
        {
            try
            {
                Trace.TraceInformation("Sending Windows Phone toast to RegistrationId " + registration.RegistrationId);
                // Define a Windows Phone toast.
                var mpnsToast =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<wp:Notification xmlns:wp=\"WPNotification\">" +
                        "<wp:Toast>" +
                            "<wp:Text1>" + messgae + "</wp:Text1>" +
                        "</wp:Toast> " +
                    "</wp:Notification>";
                await hub.SendMpnsNativeNotificationAsync(mpnsToast);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Windows Phone toast: " + ex.Message);
            }
        }

        public static async Task sendWindowsStoreNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
        {
            try
            {
                Trace.TraceInformation("Sending Windows Store toast to RegistrationId " + registration.RegistrationId);
                // Define a Windows Store toast.
                var wnsToast = "<toast><visual><binding template=\"ToastText01\">"
                    + "<text id=\"1\">" + messgae
                    + "</text></binding></visual></toast>";
                await hub.SendWindowsNativeNotificationAsync(wnsToast);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Windows Store toast: " + ex.Message);
            }
        }
    }
}
