using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Web.Http;
using ContosoMomentsMobileWeb.DataObjects;
using ContosoMomentsMobileWeb.Models;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;

namespace ContosoMomentsMobileWeb
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            HttpConfiguration config = new HttpConfiguration();
            
            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Database.SetInitializer(new MobileServiceInitializer());
        }
    }

    public class MobileServiceInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
    {
        public override void InitializeDatabase(MobileServiceContext context)
        {
            base.InitializeDatabase(context);
        }
        protected async override void Seed(MobileServiceContext context)
        {
            //Add default user and album into new DB
            Album album = new Album() { AlbumId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AlbumName = "Demo Album" };
            context.Set<Album>().Add(album);

            User user = new User() { UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), UserName = "Demo User", IsEnabled = true };
            context.Set<User>().Add(user);

            await context.SaveChangesAsync();
            base.Seed(context);
        }
    }
}

