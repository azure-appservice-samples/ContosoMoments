using System;
using System.Linq;
using ContosoMoments.Common;
using ContosoMoments.Common.Models;
using ContosoMoments.Common.Storage;
using ContosoMoments.MobileServer.Models;

namespace ContosoMoments.MobileServer.DataLogic
{
    public class ImageBusinessLogic
    {

        public Image AddImageToDB(string AlbumId, string UserId, string containerName, string fileName, bool IsMobile)
        {
            var uploadFormat = IsMobile ? "Mobile Image" : "Web Image";
            return AddImageToDB(AlbumId, UserId, containerName, fileName, uploadFormat);
        }

        public Image AddImageToDB(string AlbumId, string UserId, string containerName, string fileName, string UploadFormat)
        {
            ContosoStorage cs = new ContosoStorage();
            var ctx = new MobileServiceContext();
            var img = new Image
            {
                Album = ctx.Albums.Where(x => x.Id == AlbumId).FirstOrDefault(),
                User = ctx.Users.Where(x => x.Id == UserId).FirstOrDefault(),

                Id = Guid.NewGuid().ToString(),

                UploadFormat = UploadFormat
            };
            ctx.Images.Add(img);
            try
            {
                ctx.SaveChanges();
                return img;
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

        public Image GetImage(string id)
        {
            using (var ctx = new MobileServiceContext())
            {
                return ctx.Images.Include("User").SingleOrDefault(x => x.Id == id);
            }
        }
    }
}