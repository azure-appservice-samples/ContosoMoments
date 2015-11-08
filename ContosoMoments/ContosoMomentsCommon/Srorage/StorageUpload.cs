using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContosoMoments.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace ContosoMoments.Common.Srorage
{
    public class StorageUpload
    {
        private static bool CorsActive;
        private CloudStorageAccount storageAccount;
        private StorageCredentials storageCred;

        private static List<String> ALLOWED_CORS_ORIGINS = new List<String> { "*" };
        private static List<String> ALLOWED_CORS_HEADERS = new List<String> { "x-ms-meta-qqfilename", "Content-Type", "x-ms-blob-type", "x-ms-blob-content-type" };
        private const CorsHttpMethods ALLOWED_CORS_METHODS = CorsHttpMethods.Delete | CorsHttpMethods.Put | CorsHttpMethods.Get;
       




        public StorageUpload()
        {
            storageCred = new StorageCredentials(AppSettings.StorageAccountName, AppSettings.StorageAccountKey);
            storageAccount = new CloudStorageAccount(storageCred, true);
            SetCors(storageAccount);

        }


        public string GetUploadSasUrl(UploadRequest request)
        {

            string imageUri = string.Empty;

            // Set the URI for the Blob Storage service.
            Uri blobEndpoint = new Uri(string.Format("https://{0}.blob.core.windows.net", storageAccount));

            // Create the BLOB service client.
            CloudBlobClient blobClient = new CloudBlobClient(blobEndpoint, storageCred);

            if (request.ContainerName != null)
            {
                // Set the BLOB store container name on the item, which must be lowercase.
                request.ContainerName = request.ContainerName.ToLower();

                // Create a container, if it doesn't already exist.
                CloudBlobContainer container = blobClient.GetContainerReference(request.ContainerName);
                container.CreateIfNotExistsAsync();

                // Create a shared access permission policy. 
                BlobContainerPermissions containerPermissions = new BlobContainerPermissions();

                // Enable anonymous read access to BLOBs.
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                container.SetPermissions(containerPermissions);

                // Define a policy that gives write access to the container for 5 minutes.                                   
                SharedAccessBlobPolicy sasPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessStartTime = DateTime.UtcNow,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    Permissions = SharedAccessBlobPermissions.Write
                };

                // Get the SAS as a string.
                var sasQueryString = container.GetSharedAccessSignature(sasPolicy);

                // Set the URL used to store the image.
                imageUri = string.Format("{0}{1}/{2}", blobEndpoint.ToString(),
                   request.ContainerName, request.FileName);
            }

            // Complete the insert operation.

            return imageUri;
        }

        public bool CommitUpload(CommitBlobRequest commitRequest)
        {

            var result = false;

            commitRequest.FileName = commitRequest.FileName.ToLower();
            commitRequest.ContainerName = commitRequest.ContainerName.ToLower();
            var accountAndKey = new StorageCredentials(AppSettings.StorageAccountName, AppSettings.StorageAccountKey);
            var storageaccount = new CloudStorageAccount(accountAndKey, true);
            var blobClient = storageaccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(commitRequest.ContainerName);
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            CloudBlockBlob blob = container.GetBlockBlobReference(commitRequest.FileName);

            try
            {
                blob.PutBlockList(commitRequest.blobParts);
                blob.Properties.ContentType = commitRequest.type;
                blob.Properties.ContentDisposition = "attachment";
                blob.SetProperties();

                result = true;
            }
            catch (Exception ex)
            {
                //Trace.TraceError("BuildFileSasUrl throw excaption", ex.Message);
            }


            return result;



        }

        #region Private



        private static void SetCors(CloudStorageAccount storageAccount)
        {
            if(CorsActive)
                return;
            var blobClient = storageAccount.CreateCloudBlobClient();

            Console.WriteLine("Storage Account: " + storageAccount.BlobEndpoint);

            var newProperties = blobClient.GetServiceProperties();

            newProperties.Cors.CorsRules.Clear();

            newProperties.DefaultServiceVersion = "2013-08-15";

            var ruleWideOpenWriter = new CorsRule()
            {
                AllowedHeaders = ALLOWED_CORS_HEADERS,
                AllowedOrigins = ALLOWED_CORS_ORIGINS,
                AllowedMethods = ALLOWED_CORS_METHODS,
                MaxAgeInSeconds = (int)TimeSpan.FromDays(AppSettings.UploadSasTime).TotalSeconds
            };

            newProperties.Cors.CorsRules.Add(ruleWideOpenWriter);
            blobClient.SetServiceProperties(newProperties);
            CorsActive = true;

        }


        #endregion
    }
}
