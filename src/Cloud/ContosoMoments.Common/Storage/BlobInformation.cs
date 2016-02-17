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

        public string BlobName
        {
            get
            {
                if (null != BlobUri)
                    return BlobUri.Segments[BlobUri.Segments.Length - 1];
                else
                    return string.Empty;
            }
        }

        public string ImageId { get; set; }

        public string FileExt { get; set; }

        public string BlobNameXS
        {
            get
            {
                return BlobUri != null ? FormatBlob("xs", ImageId, FileExt) : String.Empty;
            }
        }

        public string BlobNameSM
        {
            get
            {
                return BlobUri != null ? FormatBlob("sm", ImageId, FileExt) : String.Empty;
            }
        }

        public string BlobNameMD
        {
            get
            {
                return BlobUri != null ? FormatBlob("md", ImageId, FileExt) : String.Empty;
            }
        }

        public string BlobNameLG
        {
            get
            {
                return BlobUri != null ? FormatBlob("lg", ImageId, FileExt) : String.Empty;
            }
        }

        private static string FormatBlob(string sizeKey, string imageID, string fileExt)
        {
            return string.Format("{0}-{1}/{2}.{3}", ContainerPrefix, sizeKey, imageID, fileExt);
        }
    }
}
