using ContosoMoments.Common.Models;
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
            Web.Models.ConfigModel config = new Web.Models.ConfigModel();
            string defaultAlbumId  = config.DefaultAlbumId;
            if (id == defaultAlbumId)
            {
               return  Task.Run(() => {
                   return "Default Album cannot be deleted !!";
                }
                
                );
            }
          
            var imgCtrl = new ImageController();
            var album  = Lookup(id).Queryable.First();
            foreach (var img in album.Images)
            {
               var res =  imgCtrl.DeleteImage(img.Id);
            }
            
            return DeleteAsync(id);
        }

    }
}