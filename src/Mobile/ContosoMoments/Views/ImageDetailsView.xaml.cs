using ContosoMoments.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class ImageDetailsView : ContentPage
    {
        public ImageDetailsView()
        {
            InitializeComponent();

            var tapLikeImage = new TapGestureRecognizer();
            tapLikeImage.Tapped += OnLike;
            imgLike.GestureRecognizers.Add(tapLikeImage);

            var tapSettingsImage = new TapGestureRecognizer();
            tapSettingsImage.Tapped += OnSettings;
            imgSettings.GestureRecognizers.Add(tapSettingsImage);
        }

        public async void OnLike(object sender, EventArgs e)
        {
            ImageDetailsViewModel vm = this.BindingContext as ImageDetailsViewModel;

            try {
                await vm.LikeImageAsync();
            }
            catch (Exception) {
                await DisplayAlert("Error", "'Like' functionality is not available at the moment. Please try again later", "OK");
            }
        }

        public async void OnSettings(object sender, EventArgs e)
        {
            ImageDetailsViewModel vm = this.BindingContext as ImageDetailsViewModel;
            await Navigation.PushModalAsync(new SettingView(App.Current as App));
        }
    }
}
