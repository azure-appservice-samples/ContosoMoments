using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments.Common
{
    public class TraceItem
    {
        public string Api { get; set; }  //API name

        public DateTime Time { get; set; }

        public string UserId { get; set; }

        public string AlbumId { get; set; }

        public string ImageId { get; set; }
    }
}
