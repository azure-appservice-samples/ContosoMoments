using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using ContosoMomentsMobileWeb.DataObjects;
using System.Web.Http.Controllers;
using ContosoMomentsMobileWeb.Models;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization.Json;

namespace ContosoMomentsMobileWeb.Controllers
{
    public class ImageController : ApiController
    {
        #region Consts and variables
        public const string SUPPORTED_CONTENT_TYPE = "image/jpeg";
        public const string FILE_EXT = ".jpg";

        public ApiServices Services { get; set; }
        MobileServiceContext _context;
        #endregion

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            _context = new MobileServiceContext();
        }

        // GET api/Image
        public async System.Threading.Tasks.Task<IQueryable<Image>> Get()
        {
            //try
            //{
            //    if (_context.Database.Exists())
            //    {
            //        IOrderedQueryable<Image> images = _context.Images.OrderBy(p => p.ImageId);
            //        return images.AsQueryable();
            //    }
            //    else
            //    {
            //        Trace.TraceWarning("[GET] /api/image/: Database not exists");
            //        return null;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Trace.TraceError("Exception in [GET] /api/image/ => " + ex.Message);
            //    return null;
            //}

            var pairs = this.Request.GetQueryNameValuePairs();
            string page = "";

            HttpWebRequest request = HttpWebRequest.CreateHttp("http://contosomomentswebapi.azurewebsites.net/api/image" + page);
            request.Method = "GET";
            var res = await request.GetResponseAsync();
            var stream = res.GetResponseStream();
            byte[] bytes = new byte[res.ContentLength];
            await stream.ReadAsync(bytes, 0, (int)res.ContentLength);
            string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var images = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Image>>(s);

            return images.AsQueryable();
        }

    }
}
