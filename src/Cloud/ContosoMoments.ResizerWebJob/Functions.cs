using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using ContosoMoments.Common.Models;
using ImageResizer;

namespace ContosoMoments.ResizerWebJob
{
    public class Functions
    {
        private static Dictionary<ImageSize, ResizeSettings> imageDimensionsTable = new Dictionary<ImageSize, ResizeSettings>()
        {
            { ImageSize.ExtraSmall, new ResizeSettings("maxwidth=320&maxheight=200") },
            { ImageSize.Small, new ResizeSettings("maxwidth=640&maxheight=400") },
            { ImageSize.Medium, new ResizeSettings("maxwidth=800&maxheight=600") }
        };

        public async static Task StartImageScalingAsync(
            [QueueTrigger("resizerequest")] BlobInformation blobInfo,
            [Blob("{BlobNameLG}/{Filename}")] CloudBlockBlob blobInput,
            [Blob("{BlobNameXS}/{Filename}")] CloudBlockBlob blobOutputExtraSmall,
            [Blob("{BlobNameSM}/{Filename}")] CloudBlockBlob blobOutputSmall,
            [Blob("{BlobNameMD}/{Filename}")] CloudBlockBlob blobOutputMedium)
        {
            using (var streamInput = await blobInput.OpenReadAsync()) {

                var contentType = await ResizeImage(streamInput, blobOutputExtraSmall, ImageSize.ExtraSmall);
                await ResizeImage(streamInput, blobOutputSmall, ImageSize.Small);
                await ResizeImage(streamInput, blobOutputMedium, ImageSize.Medium);

                blobInput.Properties.ContentType = contentType;
                blobInput.SetProperties();
            }
        }

        private static async Task<string> ResizeImage(Stream streamInput, CloudBlockBlob blobOutput, ImageSize size)
        {
            streamInput.Position = 0;

            using (var memoryStream = new MemoryStream()) {
                // use a memory stream, since using the blob stream directly causes InvalidOperationException due to the way image resizer works
                var instructions = new Instructions(imageDimensionsTable[size]);
                var job = new ImageJob(streamInput, memoryStream, instructions, disposeSource: false, addFileExtension: false);

                // use the advanced version of resize so that we can get the content type
                var result = ImageBuilder.Current.Build(job);

                memoryStream.Position = 0;
                await blobOutput.UploadFromStreamAsync(memoryStream);

                var contentType = result.ResultMimeType;
                blobOutput.Properties.ContentType = contentType;
                blobOutput.SetProperties();

                return contentType;
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
    }
}
