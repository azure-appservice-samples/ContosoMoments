using System.Configuration;

namespace ContosoMoments.Common
{
    public class AppSettings
    {
        public static string StorageAccountName = ConfigurationManager.AppSettings.Get("StorageAccountName");
        public static string StorageAccountKey = ConfigurationManager.AppSettings.Get("StorageAccountKey");
        public static string ResizeQueueName = ConfigurationManager.AppSettings.Get("ResizeQueueName");
        public static string UploadContainerName = ConfigurationManager.AppSettings.Get("UploadContainerName");
        public static string FacebookAuthString = ConfigurationManager.AppSettings.Get("FacebookAuthString");

    }
}
