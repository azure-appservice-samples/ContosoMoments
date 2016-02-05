namespace ContosoMoments.MobileServer.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ContosoMoments.MobileServer.Models.MobileServiceContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "ContosoMoments.MobileServer.Models.MobileServiceContext";
        }
    }
}
