using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using ContosoMomentsCommon.Models;
using Microsoft.Framework.OptionsModel;

namespace ContosoMomentsWebAPI.Model
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Album> Albums { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }

        public ApplicationDbContext(IOptions<AppSettings> appSettings)
        {
            try
            {
                if (Database.AsRelational().Exists())
                {
                    bool tableExists = true;
                    try
                    {
                        var count = Users.Count();
                        if (count < 1)
                            tableExists = false;
                    }
                    catch (Exception ex)
                    {
                        tableExists = false;
                    }

                    if (!tableExists)
                    {
                        Database.AsRelational().CreateTables();

                        User user = new User() { UserId = Guid.Parse(appSettings.Options.DefaultId), UserName = "Demo User", IsEnabled = true };
                        Users.Add(user);

                        Album album = new Album() { AlbumId = Guid.Parse(appSettings.Options.DefaultId), AlbumName = "Default Album" };
                        Albums.Add(album);

                        SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
