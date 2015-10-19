using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMomentsCommon.Models
{
    public class ImageInfo
    {
        public Guid ImageId { get; set; }

        public ImageFormats ImageFormat { get; set; }

        public IList<string> ImagePath { get; set; }

        public Guid AlbumId { get; set; }
        public string AlbumName { get; set; }

        public Guid UserId { get; set; }
        public string UserName { get; set; }

    }
}
