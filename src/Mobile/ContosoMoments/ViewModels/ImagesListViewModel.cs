using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Sync;
using PCLStorage;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ContosoMoments.ViewModels
{
    public class ImagesListViewModel : BaseViewModel
    {
        private App _app;

        public ImagesListViewModel(MobileServiceClient client, App app)
        {
            _client = client;
            _app = app;

            DeleteCommand = new DelegateCommand(OnDeleteAlbum, AlbumsListViewModel.IsRenameAndDeleteEnabled);
        }

        #region Properties
        private ObservableCollection<Image> _images;
        public ObservableCollection<Image> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                OnPropertyChanged(nameof(Images));
            }
        }

        private Album _album;
        public Album Album
        {
            get { return _album; }
            set
            {
                _album = value;
                OnPropertyChanged(nameof(Album));
            }
        }

        public ICommand DeleteCommand { get; set; }

        private string _errorMessage = null;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public Action<Image> DeleteImageViewAction { get; set; }
        #endregion

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

                App.Instance.MobileService.EventManager.Subscribe<MobileServiceEvent>(DownloadStatusObserver);
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

        private void OnDeleteAlbum(object obj)
        {
            var selectedImage = obj as Image;
            DeleteImageViewAction?.Invoke(selectedImage);
        }
    }
}