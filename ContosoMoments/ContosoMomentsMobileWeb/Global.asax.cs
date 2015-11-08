namespace ContosoMoments.MobileServices
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register();
        }
    }
}