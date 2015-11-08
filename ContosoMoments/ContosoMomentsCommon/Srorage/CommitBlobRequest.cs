using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments.Common.Srorage
{
    public class CommitBlobRequest
    {
        public Guid UserId { get; set; }
        public Guid AlbumId { get; set; }
        public string ContainerName { get; set; }
        public string FileName { get; set; }
        public string[] blobParts { get; set; }
        public string type { get; set; }
       
    }
}
