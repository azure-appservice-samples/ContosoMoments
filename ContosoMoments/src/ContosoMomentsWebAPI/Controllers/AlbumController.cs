using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ContosoMomentsCommon.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class AlbumController : Controller
    {
        // GET: api/album
        [HttpGet]
        public IEnumerable<AlbumInfo> Get()
        {
            var albums = new List<AlbumInfo>();
            albums.Add(new AlbumInfo() { AlbumId = Guid.Empty, AlbumName = "Default" });
            return albums;
        }

        // GET api/album/5
        [HttpGet("{id}")]
        public AlbumInfo Get(int id)
        {
            return new AlbumInfo() { AlbumId = Guid.Empty, AlbumName = "Default" };
        }

        // POST api/album
        [HttpPost]
        public bool Post()
        {
            var album = new AlbumInfo();
            //...

            return true;
        }

        //// PUT api/album/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]AlbumInfo metadata)
        //{
        //    //UNSUPPORTED for P1
        //}

        // DELETE api/album/5
        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            return true;
        }
    }
}
