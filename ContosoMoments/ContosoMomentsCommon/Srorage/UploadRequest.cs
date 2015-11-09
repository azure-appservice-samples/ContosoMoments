using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments.Common.Models
{
    public class UploadRequest
    {
        public string ContainerName { get; set; }   
        public string FileName { get; set; }
        public string FileType { get; set; }

    }
}
