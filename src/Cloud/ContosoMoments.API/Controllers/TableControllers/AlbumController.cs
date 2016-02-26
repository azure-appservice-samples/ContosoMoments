using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace ContosoMoments.Api
{
    public class AlbumController : TableController<Album>
    {
        private MobileServiceContext dbContext;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            dbContext = new MobileServiceContext();
            controllerContext.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            DomainManager = new EntityDomainManager<Album>(dbContext, Request, enableSoftDelete: IsSoftDeleteEnabled());
        }

        [Route("tables/Album")]
        public async Task<IQueryable<Album>> GetAllAlbum()
        {
            string currentUserId = await ManageUserController.GetUserId(Request, User);
            return Query().Where(x => x.UserId == currentUserId || x.IsDefault);
        }
        [Route("tables/Album/{id}")]
        public SingleResult<Album> GetAlbum(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        [Route("tables/Album/{id}")]
        public Task<Album> PatchAlbum(string id, Delta<Album> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Album
        [Authorize]
        [Route("tables/Album")]
        public async Task<IHttpActionResult> PostAlbum(Album item)
        {
            Album current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        [Route("tables/Album/{albumId}")]
        public async Task DeleteAlbum(string albumId)
        {
            if (albumId == new ConfigModel().DefaultAlbumId) {
                var message =
                    new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { ReasonPhrase = "Cannot delete default album" };
                throw new HttpResponseException(message);
            }

            var album = await dbContext.Albums.FindAsync(albumId);

            var domainManager = new EntityDomainManager<Image>(dbContext, Request, IsSoftDeleteEnabled());

            foreach (var img in album.Images) {
                await domainManager.DeleteAsync(img.Id);
                await ImageController.DeleteBlobAsync(img.Id);
            }

            await DeleteAsync(albumId);
        }

        public static bool IsSoftDeleteEnabled()
        {
            return Convert.ToBoolean(ConfigurationManager.AppSettings["enableSoftDelete"]);
        }
    }
}