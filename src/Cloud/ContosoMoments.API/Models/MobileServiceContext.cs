using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Tables;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace ContosoMoments.Api
{
    public class MobileServiceContext : DbContext
    {
        private const string connectionStringName = "Name=MS_TableConnectionString";

        public MobileServiceContext() : base(connectionStringName) {}

        public DbSet<Image> Images { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DeviceRegistration> DeviceRegistrations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(
               new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                   "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
            base.OnModelCreating(modelBuilder);
        }
    }

    public class ContosoMomentsDBInitializer : CreateDatabaseIfNotExists<MobileServiceContext>
    {
        private NameValueCollection appSettings = ConfigurationManager.AppSettings;

        protected override void Seed(MobileServiceContext context)
        {
            var userid = appSettings["DefaultUserId"];

            var defaultAlbum = new Album {
                Id = appSettings["DefaultAlbumId"],
                AlbumName = "Default Album",
                IsDefault = true,
                User = new User {
                    Id = userid
                },
                UserId = userid
            };

            context.Albums.Add(defaultAlbum);
            context.SaveChanges();
            base.Seed(context);
        }
    }
}