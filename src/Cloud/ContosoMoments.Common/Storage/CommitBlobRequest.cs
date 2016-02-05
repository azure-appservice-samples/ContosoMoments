using ContosoMoments.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments.Common.Storage
{
    public class CommitBlobRequest
    {
        public string UserId { get; set; }
        public bool IsMobile { get; set; }  
        public string AlbumId { get; set; }
        public string SasUrl { get; set; }
        public string[] blobParts { get; set; }
    }
    public class CommitBlobResponse
    {
        public bool Success { get; set; }

        public string ImageId { get; set; }


    }
}
