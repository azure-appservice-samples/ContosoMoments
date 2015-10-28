using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using ContosoMomentsMobileWeb.DataObjects;
using ContosoMomentsMobileWeb.Models;
using System.Web.Http.Controllers;
using System.Text;

namespace ContosoMomentsMobileWeb.Controllers
{
    public class AlbumController : ApiController
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

        // GET api/Album
        public async System.Threading.Tasks.Task<IQueryable<Album>> Get()
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["WebAPI_BaseURL"];
            string url = baseUrl + "album";

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "GET";
            var res = await request.GetResponseAsync();
            var stream = res.GetResponseStream();
            byte[] bytes = new byte[res.ContentLength];
            await stream.ReadAsync(bytes, 0, (int)res.ContentLength);
            string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var albums = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Album>>(s);

            return albums.AsQueryable();
        }

        public async System.Threading.Tasks.Task<IEnumerable<Image>> Get(string Id)
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["WebAPI_BaseURL"];
            string url = baseUrl + "album/" + Id;

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "GET";

            var res = await request.GetResponseAsync();
            var stream = res.GetResponseStream();
            byte[] bytes = new byte[res.ContentLength];
            await stream.ReadAsync(bytes, 0, (int)res.ContentLength);
            string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var images = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Image>>(s);

            return images.AsEnumerable();
        }
    }
}
