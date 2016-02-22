using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ContosoMoments.Common.Notification
{
    public class Notifier
    {

        private NotificationHubClient hub;

        private Notifier()
        {
            var testSend = false;
#if DEBUG
            testSend = true;
#endif
            var hubConnectionString = ConfigurationManager.ConnectionStrings["MS_NotificationHubConnectionString"].ToString();
            var hubName = ConfigurationManager.AppSettings["MS_NotificationHubName"];
            hub = NotificationHubClient.CreateClientFromConnectionString(hubConnectionString, hubName, testSend);
        }


        private static Notifier instance;
        public static Notifier Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Notifier();
                }

                return instance;
            }
        }

        public async Task<Installation> GetRegistration(string installationId)
        {
            return await hub.GetInstallationAsync(installationId);
        }

        public async Task RemoveRegistration(string installationId)
        {
            await hub.DeleteInstallationAsync(installationId);
            return;
        }

        public async Task<bool> SendTemplateNotification(Dictionary<string, string> notification, IEnumerable<string> tags)
        {
            NotificationOutcome outcome = null;
            try
            {
                outcome = await hub.SendTemplateNotificationAsync(notification, tags);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending template notifications: " + ex.Message);
            }

            return (outcome.Success > 0 && outcome.Failure == 0);
        }

        public async Task sendGCMNotification(string message, IEnumerable<string> tags)
        {
            try
            {
                // Define an Android notification.
                var notification = "{\"data\":{\"msg\":\"" + message + "\"}}";
                await hub.SendGcmNativeNotificationAsync(notification, tags);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Google notification: " + ex.Message);
            }
        }

        public async Task sendIOSNotification(string message, IEnumerable<string> tags)
        {
            try
            {
                // Define an iOS alert.
                var alert = "{\"aps\":{\"alert\":\"" + message + "\"}}";
                await hub.SendAppleNativeNotificationAsync(alert, tags);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending iOS alert: " + ex.Message);
            }
        }

        public async Task sendWPNotification(string message, IEnumerable<string> tags)
        {
            try
            {
                // Define a Windows Phone toast.
                var mpnsToast =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<wp:Notification xmlns:wp=\"WPNotification\">" +
                        "<wp:Toast>" +
                            "<wp:Text1>" + message + "</wp:Text1>" +
                        "</wp:Toast> " +
                    "</wp:Notification>";
                await hub.SendMpnsNativeNotificationAsync(mpnsToast, tags);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Windows Phone toast: " + ex.Message);
            }
        }

        public async Task sendWindowsStoreNotification(string message, IEnumerable<string> tags)
        {
            try
            {
                // Define a Windows Store toast.
                var wnsToast = "<toast><visual><binding template=\"ToastText01\">"
                    + "<text id=\"1\">" + message
                    + "</text></binding></visual></toast>";
                await hub.SendWindowsNativeNotificationAsync(wnsToast, tags);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Windows Store toast: " + ex.Message);
            }
        }
    }
}
