// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Owin;
using Newtonsoft.Json;
using ContosoMoments.API;
using Microsoft.Azure.Mobile.Server.Authentication;
using System.Configuration;

namespace ContosoMoments.MobileServer
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.EnableCors();
            config.MapHttpAttributeRoutes();
            config.EnableSystemDiagnosticsTracing();
            config.Formatters.JsonFormatter.SerializerSettings.Re‌​ferenceLoopHandling = ReferenceLoopHandling.Ignore;

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            Database.SetInitializer(new ContosoMomentsDBInitializer());

            // set up auth for local development
            app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions() {
                SigningKey = ConfigurationManager.AppSettings["authSigningKey"],
                ValidAudiences = new[] { ConfigurationManager.AppSettings["authAudience"] },
                ValidIssuers = new[] { ConfigurationManager.AppSettings["authIssuer"] },
                TokenHandler = config.GetAppServiceTokenHandler()
            });

            app.UseWebApi(config);         
        }
    }
}
