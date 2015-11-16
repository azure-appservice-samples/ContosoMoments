using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ContosoMoments.Common;
using ContosoMoments.Common.Models;
using ContosoMoments.Common.Queue;
using ContosoMoments.Common.Srorage;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server.Config;
using System.Threading.Tasks;

namespace ContosoMoments.MobileServer.Controllers
{

    [MobileAppController]
    public class CommitBlobController : ApiController
    {


        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<bool> Post([FromBody]CommitBlobRequest commitBlobRequest)
        {
            
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
            string fileGuidName = urldata[0].Replace(containerName +"/lg/", "").Replace(".jpg", ""); 
        
           var ibl = new ImageBusinessLogic();
            ibl.AddImageToDB(commitBlobRequest.AlbumId ,commitBlobRequest.UserId , containerName, fileGuidName + ".jpg", commitBlobRequest.IsMobile = false);

            var qm = new QueueManager();
            var blobInfo = new BlobInformation();
            blobInfo.BlobUri = cs.GetBlobUri(containerName, urldata[0].Replace(containerName , ""));
           // blobInfo.FileGuidName = fileGuidName;
            blobInfo.ImageId = fileGuidName;
            await qm.PushToQueue(blobInfo);
            return true;

        }

    }
}

