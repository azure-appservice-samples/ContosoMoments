using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using ContosoMomentsCommon;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using Microsoft.Azure.NotificationHubs;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace ContosoMomentsImagesResizerWebJob
{
    public class Functions
    {
        #region Queue handlers
        public async static Task StartImageScalingAsync([QueueTrigger("resizerequest")] BlobInformation blobInfo,
            [Blob("{BlobName}/{BlobNameLG}", FileAccess.Read)] Stream blobInput,
            [Blob("{BlobName}/{BlobNameXS}")] CloudBlockBlob blobOutputExtraSmall,
            [Blob("{BlobName}/{BlobNameSM}")] CloudBlockBlob blobOutputSmall,
            [Blob("{BlobName}/{BlobNameMD}")] CloudBlockBlob blobOutputMedium)
        {
            Trace.TraceInformation("Scaling " + blobInfo.ImageId + " to MEDIUM size");
            bool res = await scaleImage(blobInput, blobOutputMedium, ImageSizes.Medium);
            TraceInfo(blobInfo.ImageId, "MEDIUM", res);

            Trace.TraceInformation("Scaling " + blobInfo.ImageId + " to SMALL size");
            res = await scaleImage(blobInput, blobOutputSmall, ImageSizes.Small);
            TraceInfo(blobInfo.ImageId, "SMALL", res);

            Trace.TraceInformation("Scaling " + blobInfo.ImageId + " to EXTRA SMALL size");
            res = await scaleImage(blobInput, blobOutputExtraSmall, ImageSizes.ExtraSmall);
            TraceInfo(blobInfo.ImageId, "EXTRA SMALL", res);

            Trace.TraceInformation("Done processing 'resizerequest' message");
        }

        private static void TraceInfo(string imageId, string size, bool res)
        {
            if (res)
                Trace.TraceInformation("Scaling " + imageId + " to " + size + " size completed successfully");
            else
                Trace.TraceWarning("Scaling " + imageId + " to " + size + " size failed. Please see previous errors in log");
        }

        public async static Task DeleteImagesAsync([QueueTrigger("deleterequest")] BlobInformation blobInfo,
            [Blob("{BlobName}/{BlobNameLG}")] CloudBlockBlob blobLarge,
            [Blob("{BlobName}/{BlobNameXS}")] CloudBlockBlob blobExtraSmall,
            [Blob("{BlobName}/{BlobNameSM}")] CloudBlockBlob blobSmall,
            [Blob("{BlobName}/{BlobNameMD}")] CloudBlockBlob blobMedium)
        {
            try
            {
                Trace.TraceInformation("Deleting LARGE image with ImageID = " + blobInfo.ImageId);
                await blobLarge.DeleteAsync();
                Trace.TraceInformation("Deleting EXTRA SMALL image with ImageID = " + blobInfo.ImageId);
                await blobExtraSmall.DeleteAsync();
                Trace.TraceInformation("Deleting SMALL image with ImageID = " + blobInfo.ImageId);
                await blobSmall.DeleteAsync();
                Trace.TraceInformation("Deleting MEDIUM image with ImageID = " + blobInfo.ImageId);
                await blobMedium.DeleteAsync();

                Trace.TraceInformation("Done processing 'deleterequest' message");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while deleting images: " + ex.Message);
            }
        }

        //public async static Task SendPushNotificationAsync([QueueTrigger("pushnotificationrequest")] BlobInformation blobInfo)
        //{
        //    //TODO: use Mobile SDK to send push notification to owner user about blobInfo.ImageId
        //    Trace.TraceInformation("Sending push notficiations for ImageId = " + blobInfo.ImageId);

        //    string notificationHubConnectionString = ConfigurationManager.AppSettings["Microsoft.Azure.NotificationHubs.ConnectionString"];
        //    string notificationHubPath = ConfigurationManager.AppSettings["Microsoft.Azure.NotificationHubs.Path"];
        //    string dataConnection = ConfigurationManager.ConnectionStrings["DataConnection"].ConnectionString;

        //    try
        //    {
        //        string userName, containerName, userId, imageId;
        //        userName = containerName = userId = imageId = null;

        //        if (getInforFroDB(blobInfo, dataConnection, ref userName, ref containerName, ref userId, ref imageId))
        //        {
        //            //Prepare the notification and sent
        //            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(notificationHubConnectionString, notificationHubPath);

        //            string imageUrl = string.Format("{0}/lg/{1}.jpg", containerName, imageId);
        //            string messgae = string.Format("{0}! Someone likes your image at {1}", userName, imageUrl);

        //            var registrations = await hub.GetRegistrationsByTagAsync(userId, 0);

        //            foreach (var registration in registrations)
        //            {
        //                if (registration is WindowsRegistrationDescription)
        //                {
        //                    await sendWindowsStoreNotification(hub, messgae, registration);
        //                }
        //                else if (registration is MpnsRegistrationDescription)
        //                {
        //                    await sendWPNotification(hub, messgae, registration);
        //                }
        //                else if (registration is AppleRegistrationDescription)
        //                {
        //                    await sendIOSNotification(hub, messgae, registration);
        //                }
        //                else if (registration is GcmRegistrationDescription)
        //                {
        //                    await sendGCMNotification(hub, messgae, registration);
        //                }
        //                else
        //                {
        //                    Trace.TraceWarning("Cannot send push notification to UNSUPPORTED device type with RegistrationId " + registration.RegistrationId);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Trace.TraceInformation("No registered push notification users found for ImageId = " + blobInfo.ImageId);
        //        }

        //        Trace.TraceInformation("Done processing 'pushnotificationrequest' message");
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceError("Error while sending push notifications: " + ex.Message);
        //    }
        //}
        #endregion

        #region Private functionality
        private async static Task<bool> scaleImage(Stream blobInput, CloudBlockBlob blobOutput, ImageSizes imageSize)
        {
            bool retVal = true; //Assume success

            try
            {
                using (Stream output = blobOutput.OpenWrite())
                {
                    if (doScaling(blobInput, output, imageSize))
                        blobOutput.Properties.ContentType = "image/jpeg";
                    else
                        retVal = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while scaling image: " + ex.Message);
                retVal = false;
            }

            return retVal;
        }

        private static bool doScaling(Stream blobInput, Stream output, ImageSizes imageSize)
        {
            bool retVal = true; //Assume success

            try
            {
                int width = 0, height = 0;

                //TODO: get original image aspect ratio, get "priority property" (width) and calculate new height...
                switch (imageSize)
                {
                    case ImageSizes.Medium:
                        width = 800;
                        height = 480;
                        break;
                    case ImageSizes.Large:
                        width = 1024;
                        height = 768;
                        break;
                    case ImageSizes.ExtraSmall:
                        width = 320;
                        height = 200;
                        break;
                    case ImageSizes.Small:
                        width = 640;
                        height = 400;
                        break;
                }

                using (Image img = Image.FromStream(blobInput))
                {
                    //Calculate aspect ratio and new heights of scaled image
                    var widthRatio = (double)width / (double)img.Width;
                    var heightRatio = (double)height / (double)img.Height;
                    var minAspectRatio = Math.Min(widthRatio, heightRatio);
                    if (minAspectRatio > 1)
                    {
                        width = img.Width;
                        height = img.Height;
                    }
                    else
                    {
                        width = (int)(img.Width * minAspectRatio);
                        height = (int)(img.Height * minAspectRatio);
                    }

                    using (Bitmap bitmap = new Bitmap(img, width, height))
                    {
                        bitmap.Save(output, ImageFormat.Jpeg);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while saving scaled image: " + ex.Message);
                retVal = false;
            }

            return retVal;
        }

        private static bool getInforFroDB(BlobInformation blobInfo, string dataConnection, ref string userName, ref string containerName, ref string userId, ref string imageId)
        {
            bool retVal = false; //Assume no data in DB

            try
            {
                using (SqlConnection connection = new SqlConnection(dataConnection))
                {
                    // Formulate the command.
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;

                    // Specify the query to be executed.
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
                    SELECT [Image].ImageId, [Image].ContainerName, [Image].UserId, [User].UserName
                    FROM [Image]
                    INNER JOIN [User] 
                    ON [Image].UserId = [User].UserId
                    WHERE [Image].ImageId = '" + blobInfo.ImageId + "'";

                    // Open a connection to database.
                    connection.Open();

                    // Read data returned for the query.
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        imageId = blobInfo.ImageId; //reader["ImageId"];
                        userName = (string)reader["UserName"];
                        userId = ((Guid)reader["UserId"]).ToString();
                        containerName = (string)reader["ContainerName"];
                        retVal = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while getting info from DB: " + ex.Message);
            }

            return retVal;
        }

        private static async Task sendGCMNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
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

        private static async Task sendIOSNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
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

        private static async Task sendWPNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
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

        private static async Task sendWindowsStoreNotification(NotificationHubClient hub, string messgae, RegistrationDescription registration)
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
        #endregion
    }
}
