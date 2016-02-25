using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.IO;

namespace ContosoMoments.ViewModels
{
    public class ImageDetailsViewModel : BaseViewModel
    {
        public ImageDetailsViewModel(MobileServiceClient client, Models.Image image)
        {
            _client = client;
            this.Image = image;
        }

        public Models.Image Image { get; set; }
        public ICommand OpenImageCommand { set; get; }

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

        public async Task LikeImageAsync()
        {
            string body = string.Format("{{'imageId':'{0}'}}", Image.Id.ToString());
            await App.Instance.MobileService.InvokeApiAsync<string, bool>("Like", body, HttpMethod.Post, null);
        }
    }
}
