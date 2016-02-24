namespace ContosoMoments.MobileServer.Migrations
{
    using System.Data.Entity.Migrations;
    using API;
    internal sealed class Configuration : DbMigrationsConfiguration<MobileServiceContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "ContosoMoments.MobileServer.Models.MobileServiceContext";
        }
    }
}
