using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using ContosoMoments.Common.Models;
using Microsoft.Azure.Mobile.Server.Tables;

namespace ContosoMoments.MobileServer.Models
{

    public class MobileServiceContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to alter your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
        //
        // To enable Entity Framework migrations in the cloud, please ensure that the 
        // service name, set by the 'MS_MobileServiceName' AppSettings in the local 
        // Web.config, is the same as the service name when hosted in Azure.

        private const string connectionStringName = "Name=MS_TableConnectionString";

        public MobileServiceContext() : base(connectionStringName)
        {

        }

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

    public class ContosoMomentsDBInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            var userid = "11111111-1111-1111-1111-111111111111";
            var defaultAlbum = new Album
            {
                Id = "11111111-1111-1111-1111-111111111111",
                AlbumName = "Default Album",
                IsDefault=true,
                User = new User
                {
                    Email = "demo@contoso.com",
                    Id = userid,
                    IsEnabled = true
                },
                UserId = userid
                

            };

            context.Albums.Add(defaultAlbum);
            context.SaveChanges();
            base.Seed(context);
        }
    }


}
