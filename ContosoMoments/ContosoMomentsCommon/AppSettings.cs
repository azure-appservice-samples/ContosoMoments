using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;

namespace ContosoMoments.Common
{
    public class AppSettings
    {
        public static string StorageAccountName = CloudConfigurationManager.GetSetting("StorageAccountName");
        public static string StorageAccountKey = CloudConfigurationManager.GetSetting("StorageAccountKey");
        public static string StorageCustomDomainName = CloudConfigurationManager.GetSetting("StorageCustomDomainName");
        public static string UploadSasTime = CloudConfigurationManager.GetSetting("UploadSasTime");
        
    }
}
