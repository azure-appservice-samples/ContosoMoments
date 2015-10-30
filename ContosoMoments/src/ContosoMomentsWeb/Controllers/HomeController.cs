using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ContosoMomentsCommon.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ContosoMomentsWeb.Controllers
{
    public class HomeController : Controller
    {
        #region API Strings
        string imageApiBaseUrl = "api/image";
        #endregion API Strings

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public async Task<IEnumerable<Image>> GetImages(int page = 0)
        {
            IEnumerable<Image> images = new List<Image>();

            // TODO: Get result from server
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://contosomomentswebapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                var parameters = page > 0 ? imageApiBaseUrl + "?page=" + page : imageApiBaseUrl;

                HttpResponseMessage response = await client.GetAsync(parameters);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    images = JsonConvert.DeserializeObject<List<Image>>(responseString);
                }
            }

            return images;
        }

        public async void PostImage(MultipartFormDataContent data)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://contosomomentswebapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // TODO: Not accepting. Why?
                HttpResponseMessage response = await client.PostAsync(imageApiBaseUrl, data);
                if (response.IsSuccessStatusCode)
                {

                }
            }
        }
    }
}
