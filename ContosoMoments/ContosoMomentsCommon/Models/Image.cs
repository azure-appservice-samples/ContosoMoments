using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.Common.Models
{
    public class Image : EntityData
    {
        public Guid ImageId { get; set; }

        public string ImageFormat { get; set; }

        public string ContainerName { get; set; }

        public Album Album { get; set; }

        public User User { get; set; }


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
