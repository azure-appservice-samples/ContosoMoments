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
        MobileServiceClient _client;

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
    }
}
