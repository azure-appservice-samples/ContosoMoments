using System;
using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server;

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
