#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.WindowsAzure.Storage.Blob;

public class BlobInfo
{
    public string ImageId { get; set; }
}

public static async Task Run(BlobInfo blobName,
                             CloudBlockBlob blobLarge,
                             CloudBlockBlob blobExtraSmall,
                             CloudBlockBlob blobSmall,
                             CloudBlockBlob blobMedium)
{
    await blobExtraSmall.DeleteAsync();
    await blobSmall.DeleteAsync();
    await blobMedium.DeleteAsync();
    await blobLarge.DeleteAsync();
}  
