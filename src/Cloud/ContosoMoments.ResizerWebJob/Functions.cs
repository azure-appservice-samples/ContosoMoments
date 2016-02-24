using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using ContosoMoments.Common.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ContosoMoments.ResizerWebJob
{
    public class Functions
    {
        #region Queue handlers
        public async static Task StartImageScalingAsync([QueueTrigger("resizerequest")] BlobInformation blobInfo,
            [Blob("{BlobNameLG}/{Filename}")] CloudBlockBlob blobInput,
            [Blob("{BlobNameXS}/{Filename}")] CloudBlockBlob blobOutputExtraSmall,
            [Blob("{BlobNameSM}/{Filename}")] CloudBlockBlob blobOutputSmall,
            [Blob("{BlobNameMD}/{Filename}")] CloudBlockBlob blobOutputMedium)
        {
            Stream input = await blobInput.OpenReadAsync();
            scaleImage(input, blobOutputMedium, ImageSize.Medium, blobInput.Properties.ContentType);

            input.Position = 0;
            scaleImage(input, blobOutputSmall, ImageSize.Small, blobInput.Properties.ContentType);

            input.Position = 0;
            scaleImage(input, blobOutputExtraSmall, ImageSize.ExtraSmall, blobInput.Properties.ContentType);

            input.Dispose();
        }

        public async static Task DeleteImagesAsync([QueueTrigger("deleterequest")] BlobInformation blobInfo,
            [Blob("{BlobNameLG}/{Filename}")] CloudBlockBlob blobLarge,
            [Blob("{BlobNameXS}/{Filename}")] CloudBlockBlob blobExtraSmall,
            [Blob("{BlobNameSM}/{Filename}")] CloudBlockBlob blobSmall,
            [Blob("{BlobNameMD}/{Filename}")] CloudBlockBlob blobMedium)
        {
            try {
                await blobLarge.DeleteAsync();
                await blobExtraSmall.DeleteAsync();
                await blobSmall.DeleteAsync();
                await blobMedium.DeleteAsync();
            }
            catch (Exception ex) {
                Trace.TraceError("Error while deleting images: " + ex.Message);
            }
        }
        #endregion

        #region Private functionality
        private static bool scaleImage(Stream blobInput, CloudBlockBlob blobOutput, ImageSize imageSize, string contentType)
        {
            bool retVal = true; //Assume success

            try {
                using (Stream output = blobOutput.OpenWrite()) {
                    if (doScaling(blobInput, output, imageSize)) {
                        blobOutput.Properties.ContentType = contentType;
                    }
                    else
                        retVal = false;
                }
            }
            catch (Exception ex) {
                Trace.TraceError("Error while scaling image: " + ex.Message);
                retVal = false;
            }

            return retVal;
        }

        private static bool doScaling(Stream blobInput, Stream output, ImageSize imageSize)
        {
            bool retVal = true; //Assume success

            try {
                int width = 0, height = 0;

                //TODO: get original image aspect ratio, get "priority property" (width) and calculate new height...
                switch (imageSize) {
                    case ImageSize.Medium:
                        width = 800;
                        height = 480;
                        break;
                    case ImageSize.Large:
                        width = 1024;
                        height = 768;
                        break;
                    case ImageSize.ExtraSmall:
                        width = 320;
                        height = 200;
                        break;
                    case ImageSize.Small:
                        width = 640;
                        height = 400;
                        break;
                }

                using (System.Drawing.Image img = System.Drawing.Image.FromStream(blobInput)) {
                    //Calculate aspect ratio and new heights of scaled image
                    var widthRatio = (double)width / (double)img.Width;
                    var heightRatio = (double)height / (double)img.Height;
                    var minAspectRatio = Math.Min(widthRatio, heightRatio);
                    if (minAspectRatio > 1) {
                        width = img.Width;
                        height = img.Height;
                    }
                    else {
                        width = (int)(img.Width * minAspectRatio);
                        height = (int)(img.Height * minAspectRatio);
                    }

                    using (Bitmap bitmap = new Bitmap(img, width, height)) {
                        bitmap.Save(output, img.RawFormat);
                    }
                }
            }
            catch (Exception ex) {
                Trace.TraceError("Error while saving scaled image: " + ex.Message);
                retVal = false;
            }

            return retVal;
        }

        #endregion
    }
}
