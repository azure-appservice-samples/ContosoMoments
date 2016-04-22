using System;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;

const string ContainerName = "images";

private static string[] containerNames = new string[] {
    $"{ContainerName}-lg",
    $"{ContainerName}-md",
    $"{ContainerName}-sm",
    $"{ContainerName}-xs"
};

public static async Task Run(string blobName, TraceWriter log)
{
    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
    var blobClient = storageAccount.CreateCloudBlobClient();

    foreach (var name in containerNames) {
        var container = blobClient.GetContainerReference(name);
        var blob = container.GetBlobReference(blobName);
        await blob.DeleteAsync();
    }
}