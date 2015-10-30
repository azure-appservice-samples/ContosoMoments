using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ContosoMomentsMobileWeb.DataObjects
{
    public class Image : EntityData
    {
        public Guid ImageId { get; set; }

        public string ImageFormat { get; set; }

        public string ContainerName { get; set; }

        //public Guid AlbumId { get; set; }

        //public Guid UserId { get; set; }

        [ForeignKey("Id")]
        public virtual Album Album { get; set; }

        [ForeignKey("Id")]
        public virtual User User { get; set; }

        [NotMapped]
        public IDictionary<string, Uri> ImagePath
        {
            get
            {
                Dictionary<string, Uri> retVal = new Dictionary<string, Uri>();

                retVal.Add("xs", new Uri(string.Format("{0}/xs/{1}.jpg", ContainerName, ImageId.ToString())));
                retVal.Add("sm", new Uri(string.Format("{0}/sm/{1}.jpg", ContainerName, ImageId.ToString())));
                retVal.Add("md", new Uri(string.Format("{0}/md/{1}.jpg", ContainerName, ImageId.ToString())));
                retVal.Add("lg", new Uri(string.Format("{0}/lg/{1}.jpg", ContainerName, ImageId.ToString())));

                return retVal;
            }
        }
    }
}