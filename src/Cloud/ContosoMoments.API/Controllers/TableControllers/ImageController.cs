using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.OData;
using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server;
using System.Configuration;
using System;
using ContosoMoments.Common;
using System.Diagnostics;

namespace ContosoMoments.Api
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

        // GET tables/Image
        [EnableCors(origins: "*", headers: "*", methods: "*")]   
        public async Task<IQueryable<Image>> GetAllImage()
        {
            string defaultUserId = new ConfigModel().DefaultUserId;
            string currentUserId = defaultUserId;

            try {
                currentUserId = await ManageUserController.GetUserId(Request, User);
            }
            catch (Exception e) {
                Trace.WriteLine("Invalid auth token: " + e);
            }

            // return images owned by the current user or the guest user
            return Query().Where(i => i.UserId == currentUserId || i.UserId == defaultUserId);
        }

        // GET tables/Image/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SingleResult<Image> GetImage(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Image/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Authorize]
        public Task<Image> PatchImage(string id, Delta<Image> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Image
        public async Task<IHttpActionResult> PostImage(Image item)
        {
            var config = new ConfigModel();

            if (item.AlbumId == config.DefaultAlbumId) {
                item.UserId = config.DefaultUserId; // default album images can be viewed by anyone, so set to the default user
            }

            Image current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Image/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
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