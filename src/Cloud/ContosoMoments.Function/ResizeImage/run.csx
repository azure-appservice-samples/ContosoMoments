#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using ImageResizer;

public static async Task Run(
    String blobTrigger, Stream input,
    CloudBlockBlob blobOutputSmall, CloudBlockBlob blobOutputMedium, CloudBlockBlob blobOutputExtraSmall)
{
    await ResizeImage(input, blobOutputExtraSmall, ImageSize.ExtraSmall);
    await ResizeImage(input, blobOutputSmall, ImageSize.Small);
    await ResizeImage(input, blobOutputMedium, ImageSize.Medium);
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
