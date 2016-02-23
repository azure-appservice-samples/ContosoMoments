using ContosoMoments.Common;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoMoments.API.Helpers
{
    public class ImageNameResolver : IContainerNameResolver
    {
        private MobileServiceContext dbContext;
        private string storeUri;

        public const string DefaultSizeKey = "lg";

        public ImageNameResolver(string storeUri = null)
        {
            this.dbContext = new MobileServiceContext();
            this.storeUri = storeUri;
        }

        public Task<string> GetFileContainerNameAsync(string tableName, string recordId, string fileName)
        {
            string result;

            if (storeUri != null) {
                // use the storeUri parameter to get the file container          
                var split = storeUri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                result = split[0];
            }
            else {
                // use the default container
                result = string.Format("{0}-{1}", BlobInformation.ContainerPrefix, DefaultSizeKey);
            }

            return Task.FromResult(result);
        }

        public Task<IEnumerable<string>> GetRecordContainerNames(string tableName, string recordId)
        {
            var sizes = new string[] { "lg", "md", "sm", "xs" };
            var result = sizes.Select(x => GetContainerAndImageName(recordId: recordId, sizeKey: x));

            return Task.FromResult(result);
        }

        public static string GetContainerAndImageName(string recordId, string sizeKey)
        {
            // image container is in the format images-xs
            // There is a custom storage provider that will filter to only that file within the container
            return string.Format("{0}-{1}/{2}", BlobInformation.ContainerPrefix, sizeKey, recordId);
        }
    }
}