using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace ContosoMoments.Api
{
    public class CustomAzureStorageProvider : AzureStorageProvider
    {
        public CustomAzureStorageProvider() : base(GetConnectionString())
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

        internal static string GetConnectionString(string connectionStringName = Constants.StorageConnectionStringName)
        {
            if (connectionStringName == null) {
                throw new ArgumentNullException("connectionStringName");
            }

            if (connectionStringName.Length == 0) {
                throw new ArgumentException("Connection string should not be empty");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionStringSettings == null) {
                throw new ConfigurationErrorsException($"Connection string is missing: {connectionStringName}");
            }

            return connectionStringSettings.ConnectionString;
        }
    }
}