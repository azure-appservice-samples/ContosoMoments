using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using ContosoMoments.Common.Srorage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace ContosoMoments.Common.Helpers
{
    class StorageHelper
    {

        private CloudStorageAccount storageAccount;

        static List<String> ALLOWED_CORS_ORIGINS = new List<String> { "*" };
        static List<String> ALLOWED_CORS_HEADERS = new List<String> { "x-ms-meta-qqfilename", "Content-Type", "x-ms-blob-type", "x-ms-blob-content-type" };
        const CorsHttpMethods ALLOWED_CORS_METHODS = CorsHttpMethods.Delete | CorsHttpMethods.Put| CorsHttpMethods.Get ;

        const int ALLOWED_CORS_AGE_DAYS = 5;


        public StorageHelper()
        {
            var storageCred = new StorageCredentials(AppSettings.StorageAccountName, AppSettings.StorageAccountKey);
             storageAccount = new CloudStorageAccount(storageCred, true);
            configureCors(storageAccount);
          
        }



        public string GetSasUrl(string containerName)
        {

            var accountAndKey = new StorageCredentials(AppSettings.StorageAccountName, AppSettings.StorageAccountKey);
            var storageaccount = new CloudStorageAccount(accountAndKey, true);
            var blobClient = storageaccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);
            var stroageUri = container.Uri;

            var url = getSasForBlob(accountAndKey, stroageUri.AbsoluteUri, "");

            return url;

        }


        public bool Post(CommitBlobRequest commitRequest)
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

        private  String getSasForBlob(StorageCredentials credentials, String blobUri, String verb)
        {


            CloudBlockBlob blob = new CloudBlockBlob(new Uri(blobUri), credentials);
            var permission = SharedAccessBlobPermissions.Write;

            if (verb == "DELETE")
            {
                permission = SharedAccessBlobPermissions.Delete;
            }

            var sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {

                Permissions = permission,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15),
            });

            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sas);
        }

        
        private static void configureCors(CloudStorageAccount storageAccount)
        {
            var blobClient = storageAccount.CreateCloudBlobClient();

            Console.WriteLine("Storage Account: " + storageAccount.BlobEndpoint);

            var newProperties = blobClient.GetServiceProperties();
            //v/ar newProperties = CurrentProperties(blobClient);

            newProperties.DefaultServiceVersion = "2013-08-15";
            blobClient.SetServiceProperties(newProperties);

            var addRule = true;
            if (addRule)
            {
                var ruleWideOpenWriter = new CorsRule()
                {
                    AllowedHeaders = ALLOWED_CORS_HEADERS,
                    AllowedOrigins = ALLOWED_CORS_ORIGINS,
                    AllowedMethods = ALLOWED_CORS_METHODS,
                    MaxAgeInSeconds = (int)TimeSpan.FromDays(ALLOWED_CORS_AGE_DAYS).TotalSeconds
                };
                newProperties.Cors.CorsRules.Clear();
                newProperties.Cors.CorsRules.Add(ruleWideOpenWriter);
                blobClient.SetServiceProperties(newProperties);

               
            
        }

    }


        #endregion
    }
}
