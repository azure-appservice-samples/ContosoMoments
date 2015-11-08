using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMomentsCommon.Models
{
    public class Album : EntityData
    {
        public Guid AlbumId { get; set; }

        public string AlbumName { get; set; }

        public IEnumerable<Image> Images { get; set; }

        public User User { get; set; }
    }
}
