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

            this.OpenImageCommand = new Command<ImageSource>((source) =>
            {
                Device.OpenUri((Uri)source.GetValue(UriImageSource.UriProperty));
            });
        }

        public Models.Image Image { get; set; }
        public ICommand OpenImageCommand { protected set; get; }

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
    }
}
