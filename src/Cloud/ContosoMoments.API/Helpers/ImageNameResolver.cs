using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoMoments.Api
{
    public class ImageNameResolver : IContainerNameResolver
    {
        private string requestedImageSize = null;

        public const string DefaultContainerPrefix = "images";
        public const string DefaultSizeKey = "lg";

        private static string[] imageSizes = new string[] { "lg", "md", "sm", "xs" };

        public ImageNameResolver(string requestedImageSize = null)
        {
            // a value of null means that all image sizes should be retrieved
            this.requestedImageSize = requestedImageSize;

            if (requestedImageSize == null || !imageSizes.Contains(this.requestedImageSize)) {
                this.requestedImageSize = DefaultSizeKey;
            }
        }

        public Task<string> GetFileContainerNameAsync(string tableName, string recordId, string fileName)
        {
            return Task.FromResult($"{DefaultContainerPrefix}-{requestedImageSize}");
        }

        public Task<IEnumerable<string>> GetRecordContainerNames(string tableName, string recordId)
        {
            // Our custom storage provider will filter to only that file within the container
            var result = imageSizes.Select(
                x => {
                    // all image sizes except original are prefixed with the size key
                    var prefix = x == DefaultSizeKey ? "" : $"{x}-"; 
                    return $"{DefaultContainerPrefix}-{x}/{prefix}{recordId}";
                }
            );

            return Task.FromResult(result);
        }
    }
}