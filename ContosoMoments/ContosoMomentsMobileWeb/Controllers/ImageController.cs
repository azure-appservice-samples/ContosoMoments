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
            //proxy call to WebAPI site
            var pairs = this.Request.GetQueryNameValuePairs();
            int top = 0, skip = 0;
            string page = "";
            string pageSize = "";

            foreach (var pair in pairs)
            {
                switch (pair.Key)
                {
                    case "$top":
                        top = int.Parse(pair.Value);
                        break;
                    case "$skip":
                        skip = int.Parse(pair.Value);
                        break;
                    default:
                        break;
                }
            }

            if (top > 0)
                pageSize = "pageSize=" + top.ToString();

            if (skip > 0 && top > 0)
                page = "page=" + (skip / top).ToString();

            //Build Url with page & pagesize from $top $skip
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["WebAPI_BaseURL"];
            string url = baseUrl + "image";
            if (page.Length > 0)
                url += "?" + page;

            if (pageSize.Length > 0)
            {
                if (page.Length > 0)
                    url += "&" + pageSize;
                else
                    url += "?" + pageSize;
            }

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "GET";
            var res = await request.GetResponseAsync();
            var stream = res.GetResponseStream();
            byte[] bytes = new byte[res.ContentLength];
            await stream.ReadAsync(bytes, 0, (int)res.ContentLength);
            string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var images = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Image>>(s);

            return images.AsQueryable();
        }

        public async System.Threading.Tasks.Task<Image> Get(string Id)
        {
            //proxy call to WebAPI site
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["WebAPI_BaseURL"];
            string url = baseUrl + "image/" + Id;

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "GET";
            var res = await request.GetResponseAsync();
            var stream = res.GetResponseStream();
            byte[] bytes = new byte[res.ContentLength];
            await stream.ReadAsync(bytes, 0, (int)res.ContentLength);
            string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var image = Newtonsoft.Json.JsonConvert.DeserializeObject<Image>(s);

            return image;
        }

        public async System.Threading.Tasks.Task<bool> Post(string UserId, string AlbumId)
        {
            var retVal = true;

            if (Request.Content.IsMimeMultipartContent())
            {
                //proxy call to WebAPI site
                string baseUrl = System.Configuration.ConfigurationManager.AppSettings["WebAPI_BaseURL"];
                string url = baseUrl + "image?User=" + UserId + "&Album=" + AlbumId;

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                HttpWebRequest wr = HttpWebRequest.CreateHttp(url);
                wr.ContentType = "multipart/form-data; boundary=" + boundary;
                wr.Method = "POST";
                //wr.KeepAlive = true;
                //wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

                System.IO.Stream rs = wr.GetRequestStream();

                List<KeyValuePair<string, string>> nvc = new List<KeyValuePair<string, string>>();
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (var key in nvc)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key.Key, key.Value);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "file", "image.jpg", "image/jpeg");
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                var dataContent = await Request.Content.ReadAsMultipartAsync();
                if (dataContent.Contents.Count != 1)
                    throw new Exception("Cannot upload more than one file");
                
                System.IO.Stream stream = await dataContent.Contents[0].ReadAsStreamAsync();


                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
                rs.Close();

                WebResponse wresp = null;
                try
                {
                    wresp = wr.GetResponse();
                    System.IO.Stream stream2 = wresp.GetResponseStream();
                    System.IO.StreamReader reader2 = new System.IO.StreamReader(stream2);
                    retVal = bool.Parse(reader2.ReadToEnd());
                    Trace.WriteLine(string.Format("File uploaded, server response is: {0}", retVal.ToString()));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error uploading file", ex.ToString());
                    if (wresp != null)
                    {
                        wresp.Close();
                        wresp = null;
                    }
                    retVal = false;
                }
                finally
                {
                    wr = null;
                }
            }
            else
                retVal = false;


            return retVal;
        }

        public async System.Threading.Tasks.Task Delete(string Id)
        {
            //proxy call to WebAPI site
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["WebAPI_BaseURL"];
            string url = baseUrl + "image/" + Id;

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "DELETE";
            var res = await request.GetResponseAsync();
            var stream = res.GetResponseStream();
            byte[] bytes = new byte[res.ContentLength];
            await stream.ReadAsync(bytes, 0, (int)res.ContentLength);
            string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }
}
