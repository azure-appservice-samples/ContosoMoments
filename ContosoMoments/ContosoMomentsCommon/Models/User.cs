using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.Common.Models
{
    public class User : EntityData
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public bool IsEnabled { get; set; }

        public IEnumerable<Album> Type { get; set; }
    }
}
