using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoMomentsWebAPI.Model
{
    public class AppSettings
    {
        public string PageSize { get; set; }
        public string StorageConnectionString { get; set; }
        public string DefaultId { get; set; }
        public string BaseContainer { get; set; }
        public string LargeImages { get; set; }
        public string SmallImages { get; set; }
        public string ExtraSmallImages { get; set; }
        public string MediumImages { get; set; }
    }
}
