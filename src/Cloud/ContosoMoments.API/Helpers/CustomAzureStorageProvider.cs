using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ContosoMoments.API.Helpers
{
    public class CustomAzureStorageProvider : AzureStorageProvider
    {
        public CustomAzureStorageProvider(string connectionString)
            : base(connectionString)
        { }

        protected override Task<IEnumerable<CloudBlockBlob>> GetContainerFilesAsync(string containerString)
        {
            // ImageNameResolver returns the container string in the format container/file
            var containerInfo = containerString.Split('/');
            string containerName = containerInfo[0];
            string blobName = containerInfo[1];

            CloudBlobContainer container = GetContainer(containerName);

            IEnumerable<CloudBlockBlob> result = new CloudBlockBlob[] { container.GetBlockBlobReference(blobName) };
            return Task.FromResult(result);
        }
    }
}