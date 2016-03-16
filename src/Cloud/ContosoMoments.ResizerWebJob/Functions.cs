using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Collections.Generic;
using ContosoMoments.Common.Models;

namespace ContosoMoments.ResizerWebJob
{
    public class Functions
    {
        private static Dictionary<ImageSize, Tuple<int, int>> imageDimensionsTable;

        static Functions()
        {
            imageDimensionsTable = new Dictionary<ImageSize, Tuple<int, int>>();

            imageDimensionsTable[ImageSize.ExtraSmall] = Tuple.Create(320, 200);
            imageDimensionsTable[ImageSize.Small]      = Tuple.Create(640, 400);
            imageDimensionsTable[ImageSize.Medium]     = Tuple.Create(800, 480);
            imageDimensionsTable[ImageSize.Large]      = Tuple.Create(1024, 768);
        }

        public async static Task StartImageScalingAsync(
            [QueueTrigger("resizerequest")] BlobInformation blobInfo,
            [Blob("{BlobNameLG}/{Filename}")] CloudBlockBlob blobInput,
            [Blob("{BlobNameXS}/{Filename}")] CloudBlockBlob blobOutputExtraSmall,
            [Blob("{BlobNameSM}/{Filename}")] CloudBlockBlob blobOutputSmall,
            [Blob("{BlobNameMD}/{Filename}")] CloudBlockBlob blobOutputMedium)
        {
            using (var input = await blobInput.OpenReadAsync()) {
                ScaleImage(input, blobOutputMedium, ImageSize.Medium);
                ScaleImage(input, blobOutputSmall, ImageSize.Small);
                ScaleImage(input, blobOutputExtraSmall, ImageSize.ExtraSmall);
            }
        }

        public async static Task DeleteImagesAsync(
            [QueueTrigger("deleterequest")] BlobInformation blobInfo,
            [Blob("{BlobNameLG}/{Filename}")] CloudBlockBlob blobLarge,
            [Blob("{BlobNameXS}/{Filename}")] CloudBlockBlob blobExtraSmall,
            [Blob("{BlobNameSM}/{Filename}")] CloudBlockBlob blobSmall,
            [Blob("{BlobNameMD}/{Filename}")] CloudBlockBlob blobMedium)
        {
            await blobExtraSmall.DeleteAsync();
            await blobSmall.DeleteAsync();
            await blobMedium.DeleteAsync();
            await blobLarge.DeleteAsync();
        }

        private static void ScaleImage(Stream blobInput, CloudBlockBlob blobOutput, ImageSize imageSize)
        {
            using (Stream output = blobOutput.OpenWrite()) {
                var imageFormat = DoScaling(blobInput, output, imageSize);

                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                var mimeFormat = codecs.First(codec => codec.FormatID == imageFormat.Guid).MimeType;

                blobOutput.Properties.ContentType = mimeFormat;
            }
        }

        private static ImageFormat DoScaling(Stream blobInput, Stream output, ImageSize imageSize)
        {
            ImageFormat imageFormat;

            var widthHeight = imageDimensionsTable[imageSize];
            int width = widthHeight.Item1;
            int height = widthHeight.Item2;

            blobInput.Position = 0;

            using (var img = System.Drawing.Image.FromStream(blobInput)) {
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
                    imageFormat = img.RawFormat;
                }
            }

            return imageFormat;
        }
    }
}
