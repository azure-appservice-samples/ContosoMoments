using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.OData;
using ContosoMoments.Common.Models;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server;
using System.Configuration;
using System;

namespace ContosoMoments.MobileServer.Controllers.TableControllers
{
    public class AlbumController : TableController<Album>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var softDeleteEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSoftDelete"]);
            MobileServiceContext context = new MobileServiceContext();
            controllerContext.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            DomainManager = new EntityDomainManager<Album>(context, Request, enableSoftDelete: softDeleteEnabled);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]

        // GET tables/Album
        public IQueryable<Album> GetAllAlbum()
        {
            return Query(); 
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]

        // GET tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Album> GetAlbum(string id)
        {
            return Lookup(id);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]

        // PATCH tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Album> PatchAlbum(string id, Delta<Album> patch)
        {
             return UpdateAsync(id, patch);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]

        // POST tables/Album
        public async Task<IHttpActionResult> PostAlbum(Album item)
        {
            Album current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]

        // DELETE tables/Album/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteAlbum(string id)
        {
             return DeleteAsync(id);
        }

    }
}