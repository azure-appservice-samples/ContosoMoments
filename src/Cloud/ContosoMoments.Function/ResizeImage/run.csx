#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using ImageResizer;
using System.Threading;
using System.Threading.Tasks;

public static Task Run(
    String blobTrigger, CloudBlockBlob input,
    CloudBlockBlob blobOutputSmall, CloudBlockBlob blobOutputMedium, CloudBlockBlob blobOutputExtraSmall)
{
    return Task.WhenAll(
        ResizeImageAsync(input, blobOutputExtraSmall, ImageSize.ExtraSmall),
        ResizeImageAsync(input, blobOutputSmall, ImageSize.Small),
        ResizeImageAsync(input, blobOutputMedium, ImageSize.Medium));
}

#region Image dimensions

public enum ImageSize
{
    ExtraSmall, Small, Medium, Large
}

private static Dictionary<ImageSize, ResizeSettings> imageDimensionsTable = new Dictionary<ImageSize, ResizeSettings>()
{
    { ImageSize.ExtraSmall, new ResizeSettings("maxwidth=320&maxheight=200") },
    { ImageSize.Small, new ResizeSettings("maxwidth=640&maxheight=400") },
    { ImageSize.Medium, new ResizeSettings("maxwidth=800&maxheight=600") }
};

#endregion

private static async Task<string> ResizeImageAsync(CloudBlockBlob blobInput, CloudBlockBlob blobOutput, ImageSize size)
{
    using (Stream memoryStream = new MemoryStream(), streamInput = await blobInput.OpenReadAsync()) {
        // use a memory stream, since using the blob stream directly causes InvalidOperationException due to the way image resizer works
        var instructions = new Instructions(imageDimensionsTable[size]);
        var job = new ImageJob(streamInput, memoryStream, instructions, disposeSource: false, addFileExtension: false);

        // use the advanced version of resize so that we can get the content type
        var result = await Task.Run(() => ImageBuilder.Current.Build(job));

        memoryStream.Position = 0;
        await blobOutput.UploadFromStreamAsync(memoryStream);

        var contentType = result.ResultMimeType;
        blobOutput.Properties.ContentType = contentType;
        blobOutput.SetProperties();

        return contentType;
    }
}
