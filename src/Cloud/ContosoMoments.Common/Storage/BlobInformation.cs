using System;

namespace ContosoMoments.Common
{
    public class BlobInformation
    {
        public const string DefaultFileExtension = "jpg";
        public const string ContainerPrefix = "images";

        public BlobInformation(string fileExt = DefaultFileExtension)
        {
            FileExt = fileExt;
        }

        public Uri BlobUri { get; set; }

        public string FileGuidName { get; set; }

        public string ImageId { get; set; }

        public string FileExt { get; set; }

        public string BlobNameXS
        {
            get
            {
                return BlobUri != null ? String.Format("{0}-{1}", "xs") : String.Empty;
            }
        }

        public string BlobNameSM
        {
            get
            {
                return BlobUri != null ? String.Format("{0}-{1}", "sm") : String.Empty;
            }
        }

        public string BlobNameMD
        {
            get
            {
                return BlobUri != null ? String.Format("{0}-{1}", "md") : String.Empty;
            }
        }

        public string BlobNameLG
        {
            get
            {
                return BlobUri != null ? String.Format("{0}-{1}", "lg") : String.Empty;
            }
        }

        public string Filename
        {
            get
            {
                return string.Format("{0}.{2}", ImageId, FileExt);
            }
        }
    }
}
