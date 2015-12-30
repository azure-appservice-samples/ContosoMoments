using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Notifications;
using System.Configuration;

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
            var connection = ConfigurationManager.AppSettings["NotificationHubConnection"];
            hub = NotificationHubClient.CreateClientFromConnectionString(connection, "contomo", testSend);
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


        public async Task<bool> SendTemplateNotification(Dictionary<string, string> notification, IEnumerable<string> Tags)
        {
            NotificationOutcome outcome = null;
            try
            {
                //  Trace.TraceInformation("Sending Google notification toast to RegistrationId " + registration.RegistrationId);
                // Define an Android notification.
              
                outcome=await hub.SendTemplateNotificationAsync(notification,Tags);
               

            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Google notification: " + ex.Message);
            }

            return outcome != null;

             
        }

        public async Task sendGCMNotification(string message)
        {
            try
            {
               
              //  Trace.TraceInformation("Sending Google notification toast to RegistrationId " + registration.RegistrationId);
                // Define an Android notification.
                var notification = "{\"data\":{\"msg\":\"" + message + "\"}}";
                await hub.SendGcmNativeNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Google notification: " + ex.Message);
            }
        }

        public  async Task sendIOSNotification(string message, RegistrationDescription registration)
        {
            try
            {
                Trace.TraceInformation("Sending iOS alert to RegistrationId " + registration.RegistrationId);
                // Define an iOS alert.
                var alert = "{\"aps\":{\"alert\":\"" + message + "\"}}";
                await hub.SendAppleNativeNotificationAsync(alert);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending iOS alert: " + ex.Message);
            }
        }

        public  async Task sendWPNotification(string message, RegistrationDescription registration)
        {
            try
            {
                Trace.TraceInformation("Sending Windows Phone toast to RegistrationId " + registration.RegistrationId);
                // Define a Windows Phone toast.
                var mpnsToast =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<wp:Notification xmlns:wp=\"WPNotification\">" +
                        "<wp:Toast>" +
                            "<wp:Text1>" + message + "</wp:Text1>" +
                        "</wp:Toast> " +
                    "</wp:Notification>";
                await hub.SendMpnsNativeNotificationAsync(mpnsToast);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while sending Windows Phone toast: " + ex.Message);
            }
        }

        public async Task sendWindowsStoreNotification(string message, RegistrationDescription registration)
        {
            try
            {
                Trace.TraceInformation("Sending Windows Store toast to RegistrationId " + registration.RegistrationId);
                // Define a Windows Store toast.
                var wnsToast = "<toast><visual><binding template=\"ToastText01\">"
                    + "<text id=\"1\">" + message
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
