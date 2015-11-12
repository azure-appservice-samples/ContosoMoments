using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ContosoMoments.Common;
using ContosoMoments.Common.Models;
using ContosoMoments.Common.Queue;
using ContosoMoments.Common.Srorage;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server.Config;

namespace ContosoMoments.MobileServer.Controllers
{

    [MobileAppController]
    public class CommitBlobController : ApiController
    {
        string  webUri = string.Format("https://{0}.blob.core.windows.net/", AppSettings.StorageAccountName);


        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public bool Post([FromBody]CommitBlobRequest commitBlobRequest)
        {
            
            var cs = new ContosoStorage();
            if (!commitBlobRequest.IsMobile)
            {
               
                cs.CommitUpload(commitBlobRequest);
            }
            var url = commitBlobRequest.SasUrl.Replace(webUri, "");
            var urldata = url.Split('?');
            var index = urldata[0].IndexOf('/');
            
            var content = urldata[0].Split('/');


            var containerName = urldata[0].Substring(0, index);

            string fileName = urldata[0].Replace(containerName +"/", "");
          //  var sasForView = cs.GetSasUrlForView(containerName, fileName);

            var ctx = new MobileServiceContext();

            var img = new Image();
            img.Album = ctx.Albums.Where(x => x.Id == "111").FirstOrDefault();
            img.User = ctx.Users.Where(x => x.Id == "111").FirstOrDefault();
            //img.
            img.ContainerName = containerName;
            img.FileName = fileName;
            img.Resized = false;
            ctx.Images.Add(img);
            ctx.SaveChanges();

            var qm = new QueueManager();
            qm.PushToQueue(new ResizeQueueMessage());
            return true;

        }

    }
}

