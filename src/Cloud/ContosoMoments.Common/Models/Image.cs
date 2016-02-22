using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoMoments.Common.Models
{
    public class Image : EntityData
    {
        public string UploadFormat { get; set; }

        // TOOD: remove ContainerName field
        public string ContainerName { get; set; }
        public string FileName { get; set; }

        [Column("Album_Id")]
        public string AlbumId { get; set; }

        public virtual Album Album { get; set; }

        [Column("User_Id")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
