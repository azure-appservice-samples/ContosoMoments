using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ContosoMomentsCommon.Models;
using ContosoMomentsWebAPI.Model;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity;
using System.Diagnostics;

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        #region Consts and variables
        private IOptions<AppSettings> _appSettings { get; set; }
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region .ctor
        public UserController(IOptions<AppSettings> appSettings, IServiceProvider serviceProvider)
        {
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
        }
        #endregion

        #region Web APIs
        // GET api/user/5
        [HttpGet("{id}")]
        public IEnumerable<Album> Get(Guid id)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    var albumIds = context.Images.Where(p => p.UserId == id).Select(c => c.AlbumId).Distinct();
                    IQueryable<Album> albums = context.Albums.Where(p => albumIds.Contains(p.AlbumId));

                    return albums.ToList();
                }
                else
                {
                    Trace.TraceWarning("[GET] /api/user/{id}: Database not exists");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in [GET] /api/user/{id} => " + ex.Message);
                return null;
            }
        }

        // POST api/user
        [HttpPost]
        public async Task<bool> Post([FromBody]string UserName)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    var user = new User() { UserId = Guid.NewGuid(), UserName = UserName, IsEnabled = true };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();

                    return true;
                }
                else
                {
                    Trace.TraceWarning("[POST] /api/user/: Database not exists");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in [POST] /api/user/ => " + ex.Message);
                return false;
            }
        }
        
        // DELETE api/user/5
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            try
            {
                var context = _serviceProvider.GetService<ApplicationDbContext>();
                if (context.Database.AsRelational().Exists())
                {
                    IQueryable<User> user = context.Users.Where(p => p.UserId == id);
                    if (user.Count() == 1)
                    {
                        User u = user.First();
                        u.IsEnabled = false;
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    Trace.TraceWarning("[DELETE] /api/user/{id}: Database not exists");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in [DELETE] /api/user/{id} => " + ex.Message);
            }
        }
        #endregion
    }
}
