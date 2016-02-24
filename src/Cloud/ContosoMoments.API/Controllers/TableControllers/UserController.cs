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
using ContosoMoments.API;

namespace ContosoMoments.MobileServer.Controllers.TableControllers
{
    public class UserController : TableController<User>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            var softDeleteEnabled=Convert.ToBoolean(ConfigurationManager.AppSettings["enableSoftDelete"]);
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<User>(context, Request ,enableSoftDelete: softDeleteEnabled);
        }

        // GET tables/User
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IQueryable<User> GetAllUser()
        {
            return Query(); 
        }

        // GET tables/User/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SingleResult<User> GetUser(string id)
        {
            try
            {
                return Lookup(id);
            }
            catch (System.Exception ex)
            {
                 throw ex;
            }
            
        }

        // PATCH tables/User/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Authorize]
        public Task<User> PatchUser(string id, Delta<User> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/User
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Authorize]
        public async Task<IHttpActionResult> PostUser(User item)
        {
            User current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/User/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Authorize]
        public Task DeleteUser(string id)
        {
             return DeleteAsync(id);
        }

    }
}