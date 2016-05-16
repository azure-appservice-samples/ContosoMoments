using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage;
using System.Diagnostics;

namespace ContosoMoments.API.Helpers
{
    public class AzureStorageCorsHelper
    {
        public static void EnableCors(string connectionString, List<string> origins, List<string> headers, List<string> methods)
        {
            var blobClient = GetBlobClient(connectionString);
            UpdateServiceProperties(blobClient, origins, headers, methods);
        }
        public static void EnableCors(string accountName, string accountKey, List<string> origins, List<string> headers, List<string> methods)
        {
            var storageUri = new Uri($"https://{accountName}.blob.core.windows.net");
            var storageCredentials = new StorageCredentials(accountName, accountKey);

            var blobClient = GetBlobClient(storageUri, storageCredentials);
            UpdateServiceProperties(blobClient, origins, headers, methods);
        }
        private static void UpdateServiceProperties(CloudBlobClient blobClient, List<string> origins, List<string> headers, List<string> methods)
        {
            ServiceProperties props = blobClient.GetServiceProperties();

            Trace.Write(props.Cors.CorsRules.ToString());

            if (! ContainsOrigin(props.Cors.CorsRules, origins))
            {
                props.Cors.CorsRules.Add(
                    new CorsRule
                    {
                        AllowedOrigins = origins,
                        AllowedHeaders = headers,
                        AllowedMethods = ExpandCorsHttpMethods(methods),
                        MaxAgeInSeconds = 1800 // 30 minutes
                    });
                blobClient.SetServiceProperties(props);
            }
        }
        private static bool ContainsOrigin(IList<CorsRule> rules, List<string> origins)
        {
            return rules.Any(e => e.AllowedOrigins.Intersect(origins).Count() != 0);
        }
        private static CorsHttpMethods ExpandCorsHttpMethods(List<string> methods)
        {
            CorsHttpMethods allowedmethods = CorsHttpMethods.None;

            methods.All(method =>
            {
                switch (method)
                {
                    case "DELETE":
                        allowedmethods |= CorsHttpMethods.Delete;
                        return true;
                    case "GET":
                        allowedmethods |= CorsHttpMethods.Get;
                        return true;
                    case "HEAD":
                        allowedmethods |= CorsHttpMethods.Head;
                        return true;
                    case "OPTIONS":
                        allowedmethods |= CorsHttpMethods.Options;
                        return true;
                    case "POST":
                        allowedmethods |= CorsHttpMethods.Post;
                        return true;
                    case "PUT":
                        allowedmethods |= CorsHttpMethods.Put;
                        return true;
                    case "TRACE":
                        allowedmethods |= CorsHttpMethods.Trace;
                        return true;
                    default:
                        allowedmethods = CorsHttpMethods.None;
                        return true;
                }
            });

            return allowedmethods;
        }
        private static CloudBlobClient GetBlobClient(Uri storageUri, StorageCredentials creds)
        {
            return new CloudBlobClient(storageUri, creds);
        }
        private static CloudBlobClient GetBlobClient(string connectionString)
        {
            CloudStorageAccount storageAccount = null;
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                return storageAccount.CreateCloudBlobClient();
            }

            throw new Exception("Unable to create CloudStorageAccount, could not parse connectionString");
        }
    }
}
