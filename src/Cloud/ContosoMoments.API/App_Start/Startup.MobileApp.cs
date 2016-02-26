using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json;
using Owin;
using System.Configuration;
using System.Data.Entity;
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
