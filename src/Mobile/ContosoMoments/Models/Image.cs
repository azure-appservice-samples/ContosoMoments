using System.ComponentModel;
using Microsoft.WindowsAzure.MobileServices.Files;
using Newtonsoft.Json;

namespace ContosoMoments.Models
{
    public class Image : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string UploadFormat { get; set; }
        public Album Album { get; set; }
        public string AlbumId { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }

        private string _uri;
        private MobileServiceFile _file;

        [JsonIgnore]
        public MobileServiceFile File
        {
            get { return _file; }
            set
            {
                _file = value;

                if (_file != null) {
                    FileHelper.GetLocalFilePathAsync(Id, _file.Name).ContinueWith(x => this.Uri = x.Result);
                }

                OnPropertyChanged(nameof(File));
            }
        }

        [JsonIgnore]
        public string Uri
        {
            get { return _uri; }
            set
            {
                _uri = value;
                OnPropertyChanged(nameof(Uri));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
