using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using ContosoMoments.Common;
using System.Diagnostics;
using static ContosoMoments.Common.Enums.ImageSizes;
using ContosoMoments.Common.Enums;
using System.Drawing;
using System.Drawing.Imaging;

namespace ContosoMoments.ResizerWebJob
{
    public class Functions
    {
        //   private const string resizeQueue = AppSettings.ResizeQueueName;
        #region Queue handlers
        public async static Task StartImageScalingAsync([QueueTrigger("resizerequest")] BlobInformation blobInfo,
            [Blob("{BlobNameLG}/{Filename}")] CloudBlockBlob blobInput,
            [Blob("{BlobNameXS}/{Filename}")] CloudBlockBlob blobOutputExtraSmall,
            [Blob("{BlobNameSM}/{Filename}")] CloudBlockBlob blobOutputSmall,
            [Blob("{BlobNameMD}/{Filename}")] CloudBlockBlob blobOutputMedium)
        {
            Trace.TraceInformation("Blob URI: " + blobInfo.BlobUri);
            Trace.TraceInformation("Blob guid: " + blobInfo.FileGuidName);
            Trace.TraceInformation("Blob imageID: " + blobInfo.ImageId);
            Trace.TraceInformation("Blob extension: " + blobInfo.FileExt);

            Stream input = await blobInput.OpenReadAsync();
            Trace.TraceInformation("Scaling " + blobInfo.ImageId + " to MEDIUM size");
            bool res = await scaleImage(input, blobOutputMedium, Medium, blobInput.Properties.ContentType);
            TraceInfo(blobInfo.ImageId, "MEDIUM", res);

            input.Position = 0;
            Trace.TraceInformation("Scaling " + blobInfo.ImageId + " to SMALL size");
            res = await scaleImage(input, blobOutputSmall, Small, blobInput.Properties.ContentType);
            TraceInfo(blobInfo.ImageId, "SMALL", res);

            input.Position = 0;
            Trace.TraceInformation("Scaling " + blobInfo.ImageId + " to EXTRA SMALL size");
            res = await scaleImage(input, blobOutputExtraSmall, ExtraSmall, blobInput.Properties.ContentType);
            TraceInfo(blobInfo.ImageId, "EXTRA SMALL", res);


            input.Dispose();
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
        #endregion

        #region Private functionality
        private async static Task<bool> scaleImage(Stream blobInput, CloudBlockBlob blobOutput, ImageSizes imageSize, string contentType)
        {
            bool retVal = true; //Assume success

            try
            {
                using (Stream output = blobOutput.OpenWrite())
                {
                    if (doScaling(blobInput, output, imageSize))
                    {
                        blobOutput.Properties.ContentType = contentType;
                    }
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
                    case Medium:
                        width = 800;
                        height = 480;
                        break;
                    case Large:
                        width = 1024;
                        height = 768;
                        break;
                    case ExtraSmall:
                        width = 320;
                        height = 200;
                        break;
                    case Small:
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
                        bitmap.Save(output, img.RawFormat);
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
