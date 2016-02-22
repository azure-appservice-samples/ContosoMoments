using ContosoMoments.ViewModels;
using System;

using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class ImageDetailsView : ContentPage
	{
		public ImageDetailsView ()
		{
			InitializeComponent ();

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
            if (await vm.LikeImageAsync())
            {
                await DisplayAlert("Success", "Your like sent to image author.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "'Like' functionality is not available at the moment. Please try again later", "OK");
            }
        }

        public async void OnSettings(object sender, EventArgs e)
        {
            ImageDetailsViewModel vm = this.BindingContext as ImageDetailsViewModel;
            await Navigation.PushModalAsync(new SettingView());
        }
    }
}
