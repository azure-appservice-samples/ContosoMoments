using System;

namespace ContosoMoments.Common
{
    public class BlobInformation
    {
        public const string DEFAULT_FILE_EXT = "jpg";
        public BlobInformation(string fileExt)
        {
            FileExt = fileExt;
        }

        public BlobInformation()
        {
            //FileExt = DEFAULT_FILE_EXT;
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

        public string BlobNameXS
        {
            get
            {
                if (null != BlobUri)
                    return string.Format("xs/{0}.{1}", ImageId, FileExt);
                else
                    return string.Empty;
            }
        }

        public string BlobNameSM
        {
            get
            {
                if (null != BlobUri)
                    return string.Format("sm/{0}.{1}", ImageId, FileExt);
                else
                    return string.Empty;
            }
        }

        public string BlobNameMD
        {
            get
            {
                if (null != BlobUri)
                    return string.Format("md/{0}.{1}", ImageId, FileExt);
                else
                    return string.Empty;
            }
        }

        public string BlobNameLG
        {
            get
            {
                if (null != BlobUri)
                    return string.Format("lg/{0}.{1}", ImageId, FileExt);
                else
                    return string.Empty;
            }
        }

        public string ImageId { get; set; }

        string _fileExt = DEFAULT_FILE_EXT;
        public string FileExt
        {
            get
            {
                return _fileExt;
            }

            set { _fileExt = value; }
        }
    }
}
