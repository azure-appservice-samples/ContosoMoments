
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
﻿using System.Web.Http.Routing.Constraints;
﻿using ContosoMoments.AzureMobileServices.Models;
﻿using ContosoMoments.Common;
﻿using ContosoMoments.Common.Models;
﻿using ContosoMoments.Common.Queue;
﻿using ContosoMoments.Common.Srorage;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ContosoMoments.MobileServices.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class CommitBlobController : ApiController
    {
        string  webUri = string.Format("https://{0}.blob.core.windows.net", AppSettings.StorageAccountName);

        public ApiServices Services { get; set; }

        // POST: api/Default
        public bool Post([FromBody]CommitBlobRequest commitBlobRequest)
        {
            bool results;
            var cs = new ContosoStorage();
            if (!commitBlobRequest.IsMobile)
            {
               
                cs.CommitUpload(commitBlobRequest);
            }
            var url = commitBlobRequest.SasUrl.Replace(webUri, "");
            var urldata = url.Split('?');
            var content = urldata[0].Split('/');


            var fileName = content[1];
            var containerName = content[0];
            var sasForView = cs.GetSasUrlForView(containerName, fileName);

            var ctx = new MobileServiceContext();

            var img = new Image();
            img.Album = ctx.Albums.Where(x => x.Id == commitBlobRequest.AlbumId).FirstOrDefault();
            img.User = ctx.Users.Where(x => x.Id == commitBlobRequest.UserId).FirstOrDefault();
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

