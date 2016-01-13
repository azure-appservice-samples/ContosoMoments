using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using ContosoMoments.Common;
using ContosoMoments.Common.Queue;
using ContosoMoments.Common.Storage;
using ContosoMoments.MobileServer.DataLogic;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server.Config;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{

    [MobileAppController]
    public class CommitBlobController : ApiController
    {


        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<CommitBlobResponse> Post([FromBody]CommitBlobRequest commitBlobRequest)
        {
            var res = new CommitBlobResponse();
            var cs = new ContosoStorage();
            if (!commitBlobRequest.IsMobile)
            {

                cs.CommitUpload(commitBlobRequest);
            }
            var url = commitBlobRequest.SasUrl.Replace(AppSettings.StorageWebUri, "");
            var urldata = url.Split('?');
            var index = urldata[0].IndexOf('/');

            var content = urldata[0].Split('/');


            var containerName = urldata[0].Substring(0, index);
            //  var fileName = urldata[0].Replace(containerName + "/", "");
            string fileGuidName = urldata[0].Replace(containerName + "/lg/", "").Replace(".jpg", "");

            
            var ibl = new ImageBusinessLogic();
            var image = ibl.AddImageToDB(commitBlobRequest.AlbumId, commitBlobRequest.UserId, containerName, fileGuidName/* + ".jpg"*/, commitBlobRequest.IsMobile);
            if (image != null)
            {
                res.Success = true;
                res.ImageId = image.Id;                                                                                                                                                                                                                            
            }
            var qm = new QueueManager();
            var blobInfo = new BlobInformation();
            blobInfo.BlobUri = cs.GetBlobUri(containerName, urldata[0].Replace(containerName, ""));
            // blobInfo.FileGuidName = fileGuidName;
            blobInfo.ImageId = fileGuidName;
            await qm.PushToResizeQueue(blobInfo);
            return res;

        }

    }
}

