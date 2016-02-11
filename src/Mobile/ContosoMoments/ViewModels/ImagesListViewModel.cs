using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using System.Linq;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices.Files;

namespace ContosoMoments.ViewModels
{
    public class ImagesListViewModel : BaseViewModel
    {
        private App _app;

        public ImagesListViewModel(MobileServiceClient client, App app)
        {
            _client = client;
            _app = app;
        }

        private List<Image> _images;
        public List<Image> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                OnPropertyChanged("Images");
            }
        }

        private User _user;
        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private Album _album;
        public Album Album
        {
            get { return _album; }
            set
            {
                _album = value;
                OnPropertyChanged("Album");
            }
        }

        private string _ErrorMessage = null;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                _ErrorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public async Task LoadImagesAsync(string albumId)
        {
            try {
                this.Images = await _app.imageTableSync.Where(i => i.AlbumId == albumId).ToListAsync();

                foreach (var im in this.Images) {
                    var result = await _app.imageTableSync.GetFilesAsync(im);
                    im.File = result.FirstOrDefault();
                }
            }
            catch (Exception ex) {
                ErrorMessage = ex.Message;
            }
        }

        public async Task DeleteImageAsync(Image selectedImage)
        {
            await _app.imageTableSync.DeleteAsync(selectedImage);
        }

        public async Task UploadImageAsync(Stream imageStream)
        {
            try {
                var image = new Models.Image {
                    UserId = User.UserId.ToString(),
                    AlbumId = Album.AlbumId,
                    UploadFormat = "Mobile Image",
                    FileName = Guid.NewGuid().ToString()
                };

                await _app.imageTableSync.InsertAsync(image); // create a new image record
                await _app.AddImage(image, imageStream); // add the image file to the record
            }
            catch (Exception e) {
                Trace.WriteLine(e);
            }
        }
    }
}