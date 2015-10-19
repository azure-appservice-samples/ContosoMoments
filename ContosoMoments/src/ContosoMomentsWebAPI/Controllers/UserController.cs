using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ContosoMomentsCommon.Models;

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        // GET api/user/5
        [HttpGet("{id}")]
        public IEnumerable<AlbumInfo> Get(int id)
        {
            var albums = new List<AlbumInfo>();
            albums.Add(new AlbumInfo() { AlbumId = Guid.Empty, AlbumName = "Default" });
            return albums;
        }

        // POST api/user
        [HttpPost]
        public bool Post([FromBody]string UserName)
        {
            return true;
        }
        
        // DELETE api/user/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
