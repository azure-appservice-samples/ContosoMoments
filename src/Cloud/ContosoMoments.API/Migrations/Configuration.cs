using System.Data.Entity.Migrations;

namespace ContosoMoments.Api
{
    internal sealed class Configuration : DbMigrationsConfiguration<MobileServiceContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "ContosoMoments.Api.MobileServiceContext";
        }
    }
}
