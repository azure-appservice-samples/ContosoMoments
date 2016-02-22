using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.Common.Models
{
    public class Image : EntityData
    {
        public string UploadFormat { get; set; }

        [Column("Album_Id")]
        public string AlbumId { get; set; }

        public virtual Album Album { get; set; }

        [Column("User_Id")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
