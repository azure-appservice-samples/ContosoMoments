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
using System.Collections.ObjectModel;

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

        private ObservableCollection<Image> _images;
        public ObservableCollection<Image> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                OnPropertyChanged("Images");
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

        public string UserName
        {
            get { return _app.CurrentUserEmail; }
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
                this.Images = new ObservableCollection<Image>();
                         
                foreach (var i in await _app.imageTableSync.Where(i => i.AlbumId == albumId).ToEnumerableAsync()) {
                    this.Images.Add(i);
                }

                foreach (var im in this.Images) {
                    var result = await _app.imageTableSync.GetFilesAsync(im);
                    im.File = result.FirstOrDefault();

                    string filePath = await FileHelper.GetLocalFilePathAsync(im.Id, im.File.Name, _app.DataFilesPath);
                    im.ImageLoaded = await FileSystem.Current.LocalStorage.CheckExistsAsync(filePath) == ExistenceCheckResult.FileExists;
                }

                App.MobileService.EventManager.Subscribe<MobileServiceEvent>(DownloadStatusObserver);
            }
            catch (Exception ex) {
                ErrorMessage = ex.Message;
            }
        }

        private void DownloadStatusObserver(MobileServiceEvent obj)
        {
            var image = Images.Where(x => x.Id == obj.Name).FirstOrDefault();
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