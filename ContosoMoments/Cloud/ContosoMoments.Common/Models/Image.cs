using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.Common.Models
{
    public class Image : EntityData
    {
       // public Guid ImageId { get; set; }

        public string UploadFormat { get; set; }

        public string ContainerName { get; set; }
        public string FileName { get; set; }

        [Column("Album_Id")]
        public string AlbumId { get; set; }

      // public string SasUrl { get; set; }
        public virtual Album Album { get; set; }

        [Column("User_Id")]
        public string UserId { get; set; }
        public User User { get; set; }

        [NotMapped]
        public IDictionary<string, Uri> ImagePath
        {
            get
            {
                Dictionary<string, Uri> retVal = new Dictionary<string, Uri>();

                retVal.Add("xs", new Uri(string.Format("{0}/xs/{1}.jpg", ContainerName, FileName)));
                retVal.Add("sm", new Uri(string.Format("{0}/sm/{1}.jpg", ContainerName, FileName)));
                retVal.Add("md", new Uri(string.Format("{0}/md/{1}.jpg", ContainerName, FileName)));
                retVal.Add("lg", new Uri(string.Format("{0}/lg/{1}.jpg", ContainerName, FileName)));

                return retVal;
            }
        }
    }
}
