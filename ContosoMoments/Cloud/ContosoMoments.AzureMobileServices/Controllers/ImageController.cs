using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using ContosoMoments.AzureMobileServices.Models;
using ContosoMoments.Common.Models;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ContosoMoments.MobileServices.Controllers
{
    public class ImageController : TableController<Image>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Image>(context, Request, Services);
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

        // PATCH tables/Images/48D68C86-6EA6-4C25-AA33-223FC9A27959
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
        public Task DeleteImage(string id)
        {
             return DeleteAsync(id);
        }

    }
}