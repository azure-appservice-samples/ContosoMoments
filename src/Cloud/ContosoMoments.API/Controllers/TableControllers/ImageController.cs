using ContosoMoments.Common;
using ContosoMoments.Common.Models;
using ContosoMoments.Common.Queue;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace ContosoMoments.MobileServer.Controllers.TableControllers
{
    public class ImageController : TableController<Image>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            var softDeleteEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSoftDelete"]);
            DomainManager = new EntityDomainManager<Image>(context, Request, enableSoftDelete: softDeleteEnabled);
        }

        // GET tables/Images
        public IQueryable<Image> GetAllImage()
        {
            return Query();
        }

        // GET tables/Images/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Image> GetImage(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Image/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task<Image> PatchImage(string id, Delta<Image> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Image
        public async Task<IHttpActionResult> PostImage(Image item)
        {
            Image current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Images/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task DeleteImage(string id)
        {
            await DeleteBlobAsync(id);  // delete blobs associated with the image
            await DeleteAsync(id);      // delete the image record itself
        }

        public static async Task DeleteBlobAsync(string imageId)
        {
            var qm = new QueueManager();
            var blobInfo = new BlobInformation("");

            blobInfo.BlobUri = new Uri(AppSettings.StorageWebUri);
            blobInfo.ImageId = imageId;

            await qm.PushToDeleteQueue(blobInfo);
        }
    }
}