using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Threading.Tasks;

namespace ContosoMoments.Common.Storage
{
    public class ContosoStorage
    {
        Uri blobEndpoint = new Uri(string.Format("https://{0}.blob.core.windows.net", AppSettings.StorageAccountName));

        /// <summary>
        /// Gets the sas URL with upload cors.
        /// </summary>
        /// <param name="sasContainerName">Name of the sas container.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public string GetSasUrlAndSetCORS(string sasContainerName, string fileName)
        {
            var blobClient = GetCloudBlobClient(ref sasContainerName, ref fileName);
            SetCors(blobClient);
            //Get the container.  This is what we will attach the signature to.
            CloudBlobContainer container = blobClient.GetContainerReference(sasContainerName);
            container.CreateIfNotExists();
            var blob = container.GetBlockBlobReference(fileName);
            var sas = BuildSAS(container);
            var url = blobEndpoint.OriginalString + blob.Uri.PathAndQuery + sas;
            Trace.TraceInformation("End - GetSasUrlWithUploadCors for containerName: {0} and filename {1} ", sasContainerName, fileName);

            return url;
        }

        public string CommitUpload(CommitBlobRequest commitRequest)
        {
            var result = BlobInformation.DefaultFileExtension;
            var url = commitRequest.SasUrl.Replace(blobEndpoint.ToString(), "");
            var urldata = url.Split('?');
            var content = urldata[0].Split('/');

            var ContainerName = content[0];
            var FileName = urldata[0].Replace(ContainerName + "/", "");

            var accountAndKey = new StorageCredentials(AppSettings.StorageAccountName, AppSettings.StorageAccountKey);
            var storageaccount = new CloudStorageAccount(accountAndKey, true);
            var blobClient = storageaccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            CloudBlockBlob blob = container.GetBlockBlobReference(FileName);

            try
            {
                if (null != commitRequest.blobParts)
                    blob.PutBlockList(commitRequest.blobParts);

                blob.FetchAttributes();
                result = blob.Properties.ContentType.Replace("image/", "").ToLower();

                if (result == "jpeg")
                    result = BlobInformation.DefaultFileExtension;

                RenameBlob(container, FileName, FileName.Replace("temp", result));
            }
            catch (Exception)
            {
                //Trace.TraceError("BuildFileSasUrl throw excaption", ex.Message);
            }

            return result;
        }


        public Uri GetBlobUri(string sasContainerName, string fileName)
        {
            var blobClient = GetCloudBlobClient(ref sasContainerName, ref fileName);
            CloudBlobContainer container = blobClient.GetContainerReference(sasContainerName);
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            var blob = container.GetBlockBlobReference(fileName);
            return container.Uri;
        }


        public string GetDownloadUrl(string sasContainerName, string fileName)
        {
            var blobClient = GetCloudBlobClient(ref sasContainerName, ref fileName);
            //Get the container.  This is what we will attach the signature to.
            CloudBlobContainer container = blobClient.GetContainerReference(sasContainerName);
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            var blob = container.GetBlockBlobReference(fileName);
            var sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(10);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            var sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            return blob.Uri + sasBlobToken;
        }

        public string GetSasUrlForView(string sasContainerName, string fileName)
        {
            var blobClient = GetCloudBlobClient(ref sasContainerName, ref fileName);
            SetCors(blobClient);


            //Get the container.  This is what we will attach the signature to.
            CloudBlobContainer container = blobClient.GetContainerReference(sasContainerName);
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            var blob = container.GetBlockBlobReference(fileName);
            //Create the shared access permissions and policy


            var sas = BuildSAS(container);
            var url = blobEndpoint + blob.Uri.PathAndQuery + sas;

            Trace.TraceInformation("End - GetSasUrlWithUploadCors for containerName: {0} and filename {1} ", sasContainerName, fileName);


            return url;

        }

        private void RenameBlob(CloudBlobContainer container, string oldName, string newName)
        {
            var source = container.GetBlobReferenceFromServer(oldName);
            var target = container.GetBlockBlobReference(newName);

            target.StartCopy(source.Uri);

            while (target.CopyState.Status == CopyStatus.Pending)
                Task.Delay(100).Wait();

            if (target.CopyState.Status != CopyStatus.Success)
                throw new ApplicationException("Rename failed: " + target.CopyState.Status);

            source.Delete();
        }

        private static string BuildSAS(CloudBlobContainer container)
        {
            var sas = container.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddYears(2),
                Permissions =
                    SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Delete |
                    SharedAccessBlobPermissions.List
            });
            return sas;
        }

        private static CloudBlobClient GetCloudBlobClient(ref string sasContainerName, ref string fileName)
        {
            Trace.TraceInformation("Start - GetCloudBlobClient for containerName: {0} and filename {1} ", sasContainerName,
                fileName);
            sasContainerName = sasContainerName.ToLower();
            fileName = fileName.ToLower();

            var accountAndKey = new StorageCredentials(AppSettings.StorageAccountName, AppSettings.StorageAccountKey);
            var storageaccount = new CloudStorageAccount(accountAndKey, true);
            var blobClient = storageaccount.CreateCloudBlobClient();
            return blobClient;
        }

        private void ConfigureCors(ServiceProperties serviceProperties)
        {
            serviceProperties.Cors = new CorsProperties();
            serviceProperties.Cors.CorsRules.Add(new CorsRule()
            {
                AllowedHeaders = new List<string>() { "*" },
                AllowedMethods = CorsHttpMethods.Put | CorsHttpMethods.Get | CorsHttpMethods.Head | CorsHttpMethods.Post,
                AllowedOrigins = new List<string>() { "*" },
                ExposedHeaders = new List<string>() { "*" },
                MaxAgeInSeconds = 1800 // 30 minutes
            });
        }

        private void SetCors(CloudBlobClient blobClient)
        {
            var newProperties = blobClient.GetServiceProperties();
            try
            {
                ConfigureCors(newProperties);
                var blobprop = blobClient.GetServiceProperties();
                // "2011-08-18"; // null;
                blobClient.SetServiceProperties(newProperties);
            }
            catch (Exception)
            {
                //throw;
            }
        }
    }
}