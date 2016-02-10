using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ContosoMoments.Common.Models;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server.Files;

namespace ContosoMoments.API.Controllers.TableControllers
{
    public class ImageNameResolver : IContainerNameResolver
    {
        private MobileServiceContext dbContext;
        private string sizeKey;

        public ImageNameResolver(string sizeKey = "lg")
        {
            this.dbContext = new MobileServiceContext();
            this.sizeKey = sizeKey;
        }

        public async Task<string> GetFileContainerNameAsync(string tableName, string recordId, string fileName)
        {
            // filename parameter is not used, since it is always stored in the database
            var imageRecord = await dbContext.Images.FindAsync(recordId);
            return GetContainerAndImageName(imageRecord, sizeKey);
        }

        public async Task<IEnumerable<string>> GetRecordContainerNames(string tableName, string recordId)
        {
            var imageRecord = await dbContext.Images.FindAsync(recordId);

            var sizes = new string[] { "lg", "md", "sm", "xs" };
            return sizes.Select(x => GetContainerAndImageName(imageRecord, x));            
        }

        private string GetContainerAndImageName(Image image, string sizeKey)
        {
            // image container is in the format images-xs
            // There is a custom storage provider that will filter to only that file within the container
            return string.Format("images-{0}/{1}", sizeKey, image.FileName);            
        }
    }
}