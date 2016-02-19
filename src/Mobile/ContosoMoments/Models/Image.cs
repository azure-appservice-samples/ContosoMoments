using System.ComponentModel;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices.Files;
using Newtonsoft.Json;
using PCLStorage;

namespace ContosoMoments.Models
{
    public class Image : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string UploadFormat { get; set; }
        public Album Album { get; set; }
        public string AlbumId { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }

        private string _uri;
        private MobileServiceFile _file;
        private bool _imageLoaded;

        [JsonIgnore]
        public MobileServiceFile File
        {
            get { return _file; }
            set
            {
                _file = value;

                if (_file != null) {
                    FileHelper.GetLocalFilePathAsync(Id, _file.Name, App.Instance.DataFilesPath)
                        .ContinueWith(x => this.Uri = x.Result);
                }

                OnPropertyChanged(nameof(File));
            }
        }

        [JsonIgnore]
        public string Uri
        {
            get
            {
                return ImageLoaded ? _uri : "";
            }

            set
            {
                _uri = value;
                OnPropertyChanged(nameof(Uri));
            }
        }

        [JsonIgnore]
        public bool ImageLoaded
        {
            get { return _imageLoaded; }
            set
            {
                _imageLoaded = value;
                OnPropertyChanged(nameof(ImageLoaded));
                OnPropertyChanged(nameof(Uri));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"+++Image -- Id: {Id}    File: {File?.Name}   Uri: {Uri}   ImageLoaded: {ImageLoaded}";
        }
    }
}
