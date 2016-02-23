using ContosoMoments.Common.Models;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace ContosoMoments.MobileServer.Controllers.TableControllers
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

        // GET tables/Album
        public IQueryable<Album> GetAllAlbum()
        {
            return Query(); 
        }

        // GET tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Album> GetAlbum(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task<Album> PatchAlbum(string id, Delta<Album> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Album
        [Authorize]
        public async Task<IHttpActionResult> PostAlbum(Album item)
        {
            Album current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public async Task DeleteAlbum(string albumId)
        {
            Web.Models.ConfigModel config = new Web.Models.ConfigModel();
            string defaultAlbumId = config.DefaultAlbumId;
            if (albumId == defaultAlbumId) {
                var message =
                    new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) 
                    { ReasonPhrase = "Cannot delete default album" };
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