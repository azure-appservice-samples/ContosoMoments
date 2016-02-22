// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json;
using Owin;
using System.Data.Entity;
using System.Web.Http;

namespace ContosoMoments.MobileServer
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.EnableCors(new System.Web.Http.Cors.EnableCorsAttribute("*", "*", "*", "*"));
            config.MapHttpAttributeRoutes();
            config.EnableSystemDiagnosticsTracing();
            config.Formatters.JsonFormatter.SerializerSettings.Re‌​ferenceLoopHandling = ReferenceLoopHandling.Ignore;

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            Database.SetInitializer(new ContosoMomentsDBInitializer());

            app.UseWebApi(config);
        }
    }


}
