using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;

namespace ContosoMoments.Common
{
    public class AppSettings
    {
        public static string StorageAccountName = ConfigurationManager.AppSettings.Get("StorageAccountName");
        public static string StorageAccountKey = ConfigurationManager.AppSettings.Get("StorageAccountKey");
        public static int UploadSasTime = int.Parse(ConfigurationManager.AppSettings.Get("UploadSasDaysTime"));
        
    }
}
