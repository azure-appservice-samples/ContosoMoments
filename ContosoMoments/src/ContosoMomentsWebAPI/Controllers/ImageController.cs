using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using ContosoMomentsWebAPI.Model;
using ContosoMomentsCommon.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class ImageController : Controller
    {
        private IOptions<AppSettings> AppSettings { get; set; }

        public ImageController(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings;
        }


        // GET: api/image?page=5
        [HttpGet]
        public IEnumerable<ImageInfo> Get([FromQuery] int? page)
        {
            return new List<ImageInfo>();
        }

        // GET: api/image/3
        [HttpGet("{id}")]
        public ImageInfo Get(int id)
        {
            return new ImageInfo();
        }

        // POST api/image?User=000-0000-0000..&Album=111-111-111...
        [HttpPost]
        public async Task<bool> Post([FromQuery]string User, [FromQuery]string Album)
        {
            var stream = Request.Body;


            return true;
        }

        // DELETE api/image/5
        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            return true;
        }
    }
}
