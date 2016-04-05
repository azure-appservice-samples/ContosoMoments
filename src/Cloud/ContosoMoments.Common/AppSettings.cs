using Microsoft.Azure;

namespace ContosoMoments.Common
{
    public class AppSettings
    {
        public static string StorageAccountName = CloudConfigurationManager.GetSetting("StorageAccountName");
        public static string StorageAccountKey = CloudConfigurationManager.GetSetting("StorageAccountKey");
        public static string ResizeQueueName = CloudConfigurationManager.GetSetting("ResizeQueueName");
        public static string DeleteQueueName = CloudConfigurationManager.GetSetting("DeleteQueueName");
        public static string UploadContainerName = CloudConfigurationManager.GetSetting("UploadContainerName");
        public static string DefaultAlbumId = CloudConfigurationManager.GetSetting("DefaultAlbumId");
        public static string DefaultUserId = CloudConfigurationManager.GetSetting("DefaultUserId");
        public static string ServiceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
        public static string StorageWebUri = string.Format("https://{0}.blob.core.windows.net/", StorageAccountName);
        public static string DefaultServiceUrl = CloudConfigurationManager.GetSetting("DefaultServiceUrl");
        public static string FunctionsBaseUri = CloudConfigurationManager.GetSetting("FunctionAppBaseUri");
        public static string FunctionPathAndSecret = CloudConfigurationManager.GetSetting("FunctionPathAndSecret");
    }
}
