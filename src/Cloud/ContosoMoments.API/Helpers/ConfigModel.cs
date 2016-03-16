using System.Configuration;

namespace ContosoMoments.Api
{
    public class ConfigModel
    {
        private string GetConfigValue(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
        }
        public string DefaultAlbumId
        {
            get { return GetConfigValue("DefaultAlbumId"); }
        }
        public string DefaultUserId
        {
            get { return GetConfigValue("DefaultUserId"); }
        }

        public string DefaultServiceUrl
        {
            get { return GetConfigValue("DefaultServiceUrl"); }
        }
    }
}