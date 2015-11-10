using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using ContosoMoments.AzureMobileServices.Models;
using ContosoMoments.Common.Models;

namespace ContosoMoments.MobileServices.Controllers
{
    public class AlbumController : TableController<Album>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Album>(context, Request, Services);
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
        public Task<Album> PatchAlbum(string id, Delta<Album> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Album
        public async Task<IHttpActionResult> PostAlbum(Album item)
        {
            Album current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteAlbum(string id)
        {
             return DeleteAsync(id);
        }

    }
}