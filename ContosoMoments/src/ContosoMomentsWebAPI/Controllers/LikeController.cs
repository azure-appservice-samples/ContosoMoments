using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ContosoMomentsWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class LikeController : Controller
    {
        // POST api/like { imageId }
        [HttpPost]
        public void Post([FromBody]Guid ImageId)
        {
            System.Diagnostics.Debug.WriteLine(ImageId.ToString());
        }
    }
}
