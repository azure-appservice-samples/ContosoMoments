using System;

namespace ContosoMoments.Common.Models
{
    public class BlobInformation
    {
        public const string DefaultFileExtension = "jpg";
        public const string DefaultContainerPrefix = "images";

        public BlobInformation(string fileExt = DefaultFileExtension)
        {
            FileExt = fileExt;
        }

        public Uri BlobUri { get; set; }

        public string ImageId { get; set; }

        public string FileExt { get; set; }

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

        public string FileName
        {
            get
            {
                if (FileExt.Length > 0)
                    return $"{ImageId}.{FileExt}";

                return ImageId;
            }
        }
    }
}
