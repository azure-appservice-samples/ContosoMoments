using System.Configuration;

namespace ContosoMoments.Common
{
    public class AppSettings
    {
        public static string StorageAccountName = ConfigurationManager.AppSettings.Get("StorageAccountName");
        public static string StorageAccountKey = ConfigurationManager.AppSettings.Get("StorageAccountKey");
        public static int UploadSasTime = int.Parse(ConfigurationManager.AppSettings.Get("UploadSasDaysTime"));
        public static string ResizeQueueName = ConfigurationManager.AppSettings.Get("ResizeQueueName");
        
    }
}
