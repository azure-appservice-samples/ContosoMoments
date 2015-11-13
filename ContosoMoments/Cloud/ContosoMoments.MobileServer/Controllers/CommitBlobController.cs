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
        string  webUri = string.Format("https://{0}.blob.core.windows.net/", AppSettings.StorageAccountName);


        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<bool> Post([FromBody]CommitBlobRequest commitBlobRequest)
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

            string fileName = urldata[0].Replace(containerName +"/lg/", "");
            fileName = fileName.Replace(".jpg", "");
            var sasForView = cs.GetSasUrlForView(containerName, fileName);

            var ctx = new MobileServiceContext();

            var img = new Image();
            img.Album = ctx.Albums.Where(x => x.Id == commitBlobRequest.AlbumId).FirstOrDefault();
            img.User = ctx.Users.Where(x => x.Id == commitBlobRequest.UserId).FirstOrDefault();
            img.Id = Guid.NewGuid().ToString();
            img.ImageFormat = commitBlobRequest.IsMobile ? "Mobile Image" : "Web Image";
            img.ContainerName = webUri + containerName;
            img.FileName = fileName;
            img.Resized = false;
            ctx.Images.Add(img);
            try
            {
                ctx.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }

            var qm = new QueueManager();
            var blobInfo = new BlobInformation();
            blobInfo.BlobUri = cs.GetBlobUri(containerName, urldata[0].Replace(containerName + "/", ""));
            blobInfo.ImageId = fileName;
            await qm.PushToQueue(blobInfo);
            return true;

        }

    }
}

