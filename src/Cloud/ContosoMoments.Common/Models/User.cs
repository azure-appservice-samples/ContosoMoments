using Microsoft.Azure.Mobile.Server;
using System.Collections.Generic;

namespace ContosoMoments.Common.Models
{
    public class User : EntityData
    {
       // public Guid UserId { get; set; }

        public string Email { get; set; }

        public bool IsEnabled { get; set; }

        public IEnumerable<Album> Type { get; set; }
    }
}
