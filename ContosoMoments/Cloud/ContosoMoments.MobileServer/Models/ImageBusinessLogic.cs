using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContosoMoments.Common;
using ContosoMoments.Common.Models;
using ContosoMoments.Common.Srorage;

namespace ContosoMoments.MobileServer.Models
{
    public class ImageBusinessLogic
    {

      

        public void AddImageToDB(string AlbumId, string UserId, string containerName, string fileGuidName,
            string fileName, bool IsMobile)
        {

            var uploadFormat = IsMobile ? "Mobile Image" : "Web Image";
            AddImageToDB(AlbumId, UserId, containerName, fileGuidName, fileName, uploadFormat);
        }

        public void AddImageToDB(string AlbumId, string UserId, string containerName, string fileGuidName, string fileName, string UploadFormat)
        {
            ContosoStorage cs = new ContosoStorage();
            var ctx = new MobileServiceContext();
            var img = new Image
            {
                Album = ctx.Albums.Where(x => x.Id == AlbumId).FirstOrDefault(),
                User = ctx.Users.Where(x => x.Id == UserId).FirstOrDefault(),
                Id = Guid.NewGuid().ToString(),
                UploadFormat = UploadFormat,
                ContainerName = AppSettings.StorageWebUri + containerName,
                FileGuidName = fileGuidName,
                Resized = false,
                LargeFileUrl = cs.GetDownloadUrl(containerName, fileName)
            };
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
        }
    }
}