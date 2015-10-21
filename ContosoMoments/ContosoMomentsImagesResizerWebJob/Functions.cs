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

            System.Diagnostics.Debug.WriteLine("Scaling " + blobInfo.ImageId + " to MEDIUM size");
            await scaleImage(blobInput, blobOutputMedium, ImageSizes.Medium);
            System.Diagnostics.Debug.WriteLine("Scaling " + blobInfo.ImageId + " to SMALL size");
            await scaleImage(blobInput, blobOutputSmall, ImageSizes.Small);
            System.Diagnostics.Debug.WriteLine("Scaling " + blobInfo.ImageId + " to EXTRA SMALL size");
            await scaleImage(blobInput, blobOutputExtraSmall, ImageSizes.ExtraSmall);

            System.Diagnostics.Debug.WriteLine("Done processing 'resizerequest' message");
        }

        public async static Task DeleteImagesAsync([QueueTrigger("deleterequest")] BlobInformation blobInfo,
            [Blob("{BlobName}/{BlobNameLG}")] CloudBlockBlob blobLarge,
            [Blob("{BlobName}/{BlobNameXS}")] CloudBlockBlob blobExtraSmall,
            [Blob("{BlobName}/{BlobNameSM}")] CloudBlockBlob blobSmall,
            [Blob("{BlobName}/{BlobNameMD}")] CloudBlockBlob blobMedium)
        {
            System.Diagnostics.Debug.WriteLine("Deleting LARGE image with ImageID = " + blobInfo.ImageId);
            await blobLarge.DeleteAsync();
            System.Diagnostics.Debug.WriteLine("Deleting EXTRA SMALL image with ImageID = " + blobInfo.ImageId);
            await blobExtraSmall.DeleteAsync();
            System.Diagnostics.Debug.WriteLine("Deleting SMALL image with ImageID = " + blobInfo.ImageId);
            await blobSmall.DeleteAsync();
            System.Diagnostics.Debug.WriteLine("Deleting MEDIUM image with ImageID = " + blobInfo.ImageId);
            await blobMedium.DeleteAsync();

            System.Diagnostics.Debug.WriteLine("Done processing 'deleterequest' message");
        }

        public async static Task SendPushNotificationAsync([QueueTrigger("pushnotificationrequest")] BlobInformation blobInfo)
        {
            System.Diagnostics.Debug.WriteLine("Sending likes for ImageId = " + blobInfo.ImageId);
            //TODO: use Mobile SDK to send push notification about blobInfo.ImageId
        }
        #endregion

        #region Private functionality
        private async static Task scaleImage(Stream blobInput, CloudBlockBlob blobOutput, ImageSizes imageSize)
        {
            using (Stream output = blobOutput.OpenWrite())
            {
                doScaling(blobInput, output, imageSize);
                blobOutput.Properties.ContentType = "image/jpeg";
            }
        }

        private static void doScaling(Stream blobInput, Stream output, ImageSizes imageSize)
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
        #endregion
    }
}
