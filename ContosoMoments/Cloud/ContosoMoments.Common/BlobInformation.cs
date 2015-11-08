using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMomentsCommon
{
    public class BlobInformation
    {
        public Uri BlobUri { get; set; }

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
                    return string.Format("xs/{0}.jpg", ImageId);
                else
                    return string.Empty;
            }
        }

        public string BlobNameSM
        {
            get
            {
                if (null != BlobUri)
                    return string.Format("sm/{0}.jpg", ImageId);
                else
                    return string.Empty;
            }
        }

        public string BlobNameMD
        {
            get
            {
                if (null != BlobUri)
                    return string.Format("md/{0}.jpg", ImageId);
                else
                    return string.Empty;
            }
        }

        public string BlobNameLG
        {
            get
            {
                if (null != BlobUri)
                    return string.Format("lg/{0}.jpg", ImageId);
                else
                    return string.Empty;
            }
        }

        public string ImageId { get; set; }
    }
}
