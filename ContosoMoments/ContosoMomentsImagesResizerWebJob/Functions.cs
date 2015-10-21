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

        public async static Task SendPushNotificationAsync([QueueTrigger("pushnotificationrequest")] BlobInformation blobInfo)
        {
            Trace.TraceInformation("Sending likes for ImageId = " + blobInfo.ImageId);
            //TODO: use Mobile SDK to send push notification about blobInfo.ImageId
        }
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

                switch (imageSize)
                {
                    case ImageSizes.Large:
                        width = 800;
                        height = 480;
                        break;
                    case ImageSizes.Medium:
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
        #endregion
    }
}
