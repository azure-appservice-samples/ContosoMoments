using System;
using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoMoments.Common.Models
{
    public class Album : EntityData
    {
       // public Guid AlbumId { get; set; }

        public string AlbumName { get; set; }

        public bool IsDefault { get; set; }

        public virtual ICollection<Image> Images { get; set; }

        public virtual User User { get; set; }

        [Column("User_Id")]
        public string UserId { get; set; }
    }
}
