using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ContosoMoments.ViewModels
{
    public class ImageDetailsViewModel : BaseViewModel
    {
        public ImageDetailsViewModel(MobileServiceClient client, Models.Image image)
        {
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
