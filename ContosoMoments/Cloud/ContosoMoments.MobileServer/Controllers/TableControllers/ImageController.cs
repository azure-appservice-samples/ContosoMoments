using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.OData;
using ContosoMoments.Common.Models;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.MobileServer.Controllers.TableControllers
{
    public class ImageController : TableController<Image>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {

           

            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Image>(context, Request, enableSoftDelete: true);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]

        // GET tables/Images
        public IQueryable<Image> GetAllImage()
        {
            return Query();              
        }

        // GET tables/Images/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SingleResult<Image> GetImage(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Images/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public Task<Image> PatchImage(string id, Delta<Image> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Images
        public async Task<IHttpActionResult> PostImage(Image item)
        {
            Image current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Images/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public Task DeleteImage(string id)
        {
             return DeleteAsync(id);
        }

    }
}