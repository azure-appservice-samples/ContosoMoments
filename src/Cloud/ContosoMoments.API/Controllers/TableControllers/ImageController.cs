using ContosoMoments.Common;
using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

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
            return Query().Where(i => i.UserId == currentUserId || i.UserId == defaultUserId).Where(i => i.IsVisible);
        }

        // GET tables/Image/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Image> GetImage(string id)
        {
            return 
                SingleResult.Create<Image>(
                    Lookup(id).Queryable.Where(i => i.IsVisible));
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
            var config = new ConfigModel();

            if (item.AlbumId == config.DefaultAlbumId) {
                item.UserId = config.DefaultUserId; // public album images can be viewed by anyone, so set to the default user

                if (AppSettings.PublicAlbumRequiresAuth) {
                    // if not logged in with AAD, images in public album should not be visible
                    item.IsVisible = await ManageUserController.IsAadLogin(Request, User);
                }
            }

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
            await QueueManager.PushToDeleteQueue(imageId);
        }
    }
}