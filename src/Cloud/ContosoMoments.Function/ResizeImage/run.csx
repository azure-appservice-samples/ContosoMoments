#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using ImageResizer;

public static void Run(
    String blobTrigger, Stream input,
    Stream blobOutputSmall, Stream blobOutputMedium, Stream blobOutputExtraSmall,
    TraceWriter log)
{
    log.Verbose(blobTrigger);

    ResizeImage(input, blobOutputExtraSmall, ImageSize.ExtraSmall);
    ResizeImage(input, blobOutputSmall, ImageSize.Small);
    ResizeImage(input, blobOutputMedium, ImageSize.Medium);
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

private static void ResizeImage(Stream streamInput, Stream output, ImageSize size)
{
    streamInput.Position = 0;

    var instructions = new Instructions(imageDimensionsTable[size]);
    var job = new ImageJob(streamInput, output, instructions, disposeSource: false, addFileExtension: false);

    // use the advanced version of resize so that we can get the content type
    var result = ImageBuilder.Current.Build(job);
    
    // TODO: write blob content type
    // var contentType = result.ResultMimeType;
    // blobOutput.Properties.ContentType = contentType;
    // blobOutput.SetProperties();
}
