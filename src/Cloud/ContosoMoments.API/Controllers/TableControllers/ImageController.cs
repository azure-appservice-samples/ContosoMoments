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

        // PATCH tables/Images/48D68C86-6EA6-4C25-AA33-223FC9A27959
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
            // TODO: delete images associated with the record

            //var image = Lookup(id).Queryable.First();
            //var filenameParts = image.FileName.Split('.');
            //var filename = filenameParts[0];
            //var fileExt = filenameParts[1];
            //var containerName = image.ContainerName;

            //var qm = new QueueManager();
            //var blobInfo = new BlobInformation(fileExt);
            //blobInfo.BlobUri = new Uri(containerName);
            //blobInfo.ImageId = filename;
            //await qm.PushToDeleteQueue(blobInfo);

            await DeleteAsync(id);
        }
    }
}