using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Linq;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using PCLStorage;

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
                    string filePath = await FileHelper.GetLocalFilePathAsync(im.Id, im.File.Name);

                    im.ImageLoaded = await FileSystem.Current.LocalStorage.CheckExistsAsync(
                        filePath) == ExistenceCheckResult.FileExists;
                    Debug.WriteLine($"ImageLoaded: {im.Id}    {im.ImageLoaded}");
                }

                App.MobileService.EventManager.Subscribe<MobileServiceEvent>(DownloadStatusObserver);
            }
            catch (Exception ex) {
                ErrorMessage = ex.Message;
            }
        }

        private void DownloadStatusObserver(MobileServiceEvent obj)
        {
            var image = Images.Find(x => x.Id == obj.Name);
            Debug.WriteLine($"Image download event: {image?.Id}");

            if (image != null) {
                image.ImageLoaded = true;
            }
        }

        public async Task DeleteImageAsync(Image selectedImage)
        {
            await _app.imageTableSync.DeleteAsync(selectedImage);
        }
    }
}