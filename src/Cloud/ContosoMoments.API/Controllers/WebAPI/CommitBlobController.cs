using ContosoMoments.Common;
using ContosoMoments.Common.Queue;
using ContosoMoments.Common.Storage;
using ContosoMoments.MobileServer.DataLogic;
using Microsoft.Azure.Mobile.Server.Config;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{

    [MobileAppController]
    public class CommitBlobController : ApiController
    {
        public async Task<CommitBlobResponse> Post([FromBody]CommitBlobRequest commitBlobRequest)
        {
            var res = new CommitBlobResponse();
            var cs = new ContosoStorage();
            string fileExt = BlobInformation.DEFAULT_FILE_EXT;

            fileExt = cs.CommitUpload(commitBlobRequest);

            var url = commitBlobRequest.SasUrl.Replace(AppSettings.StorageWebUri, "");
            var urldata = url.Split('?');
            var index = urldata[0].IndexOf('/');
            var content = urldata[0].Split('/');
            var containerName = urldata[0].Substring(0, index);
            string fileGuidName = urldata[0].Replace(containerName + "/lg/", "").Replace(".temp", "");

            var ibl = new ImageBusinessLogic();
            var image = ibl.AddImageToDB(commitBlobRequest.AlbumId, commitBlobRequest.UserId, containerName, fileGuidName + "." + fileExt, commitBlobRequest.IsMobile);
            if (image != null)
            {
                res.Success = true;
                res.ImageId = image.Id;                                                                                                                                                                                                                            
            }

            var qm = new QueueManager();
            var blobInfo = new BlobInformation(fileExt);
            blobInfo.BlobUri = cs.GetBlobUri(containerName, urldata[0].Replace(containerName, ""));
            blobInfo.ImageId = fileGuidName;
            await qm.PushToResizeQueue(blobInfo);
            return res;
        }

    }
}

