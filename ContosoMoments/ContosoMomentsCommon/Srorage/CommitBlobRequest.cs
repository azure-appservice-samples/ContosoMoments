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
        public string SasUrl { get; set; }
        public string[] blobParts { get; set; }
       
       
    }
}
