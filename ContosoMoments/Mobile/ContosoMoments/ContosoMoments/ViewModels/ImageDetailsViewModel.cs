using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ContosoMoments.ViewModels
{
    public class ImageDetailsViewModel : BaseViewModel
    {
        public ImageDetailsViewModel(MobileServiceClient client, Models.Image image)
        {
            _client = client;
            this.Image = image;
            _UserName = "Demo User";
            _AlbumName = "Demo Album";

            this.OpenImageCommand = new Command<ImageSource>((source) =>
            {
                Device.OpenUri((Uri)source.GetValue(UriImageSource.UriProperty));
            });
        }

        public Models.Image Image { get; set; }
        public ICommand OpenImageCommand { protected set; get; }

        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                _UserName = value;
                OnPropertyChanged("UserName");
            }
        }

        private string _AlbumName;
        public string AlbumName
        {
            get { return _AlbumName; }
            set
            {
                _AlbumName = value;
                OnPropertyChanged("AlbumName");
            }
        }
    }
}
