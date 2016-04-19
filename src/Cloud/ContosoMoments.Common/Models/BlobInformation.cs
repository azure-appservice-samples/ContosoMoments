using System;

namespace ContosoMoments.Common.Models
{
    public class BlobInformation
    {
        public const string DefaultContainerPrefix = "images";

        public string ImageId { get; set; }

        private string containerPrefix;

        public string ContainerPrefix
        {
            get { return containerPrefix ?? DefaultContainerPrefix; }
            set { containerPrefix = value; }
        }

        public string BlobNameXS
        {
            get { return $"{ContainerPrefix}-xs"; }
        }

        public string BlobNameSM
        {
            get { return $"{ContainerPrefix}-sm"; }
        }

        public string BlobNameMD
        {
            get { return $"{ContainerPrefix}-md"; }
        }

        public string BlobNameLG
        {
            get { return $"{ContainerPrefix}-lg"; }
        }
    }
}
