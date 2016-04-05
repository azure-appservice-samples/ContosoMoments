using ContosoMoments.API.Helpers;
using ContosoMoments.Common;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ContosoMoments.Api
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.EnableCors(new EnableCorsAttribute("*", "*", "*", "*"));
            config.MapHttpAttributeRoutes();
            config.EnableSystemDiagnosticsTracing();
            config.Formatters.JsonFormatter.SerializerSettings.Re‌​ferenceLoopHandling = ReferenceLoopHandling.Ignore;

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            Database.SetInitializer(new ContosoMomentsDBInitializer());

            ConfigureStorage();

            // set up auth for local development
            //app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions() {
            //    SigningKey = ConfigurationManager.AppSettings["authSigningKey"],
            //    ValidAudiences = new[] { ConfigurationManager.AppSettings["authAudience"] },
            //    ValidIssuers = new[] { ConfigurationManager.AppSettings["authIssuer"] },
            //    TokenHandler = config.GetAppServiceTokenHandler()
            //});

            // Increases the HTTP Connection Pool.
            ServicePointManager.DefaultConnectionLimit = 100;

            app.UseWebApi(config);
        }

        private static void ConfigureStorage()
        {
            string connectionString =
                ConfigurationManager
                .ConnectionStrings["MS_AzureStorageAccountConnectionString"]
                .ConnectionString;

            var headers = new List<string>();
            headers.Add("*");

            var origins = new List<string>();

            string serviceUri = AppSettings.DefaultServiceUrl.ToLower().TrimEnd('/');

            origins.Add(serviceUri);

            var methods = new List<string>();
            methods.Add(HttpMethod.Get.ToString());
            methods.Add(HttpMethod.Head.ToString());
            methods.Add(HttpMethod.Post.ToString());
            methods.Add(HttpMethod.Put.ToString());

            AzureStorageCorsHelper.EnableCors(
                connectionString
                , origins
                , headers
                , methods);
        }
    }

}
