using ContosoMoments.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class ImagesList : ContentPage
    {
        ImagesListViewModel viewModel = new ImagesListViewModel(App.MobileService);

        public ImagesList()
        {
            InitializeComponent();

            BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            var tapUploadImage = new TapGestureRecognizer();
            tapUploadImage.Tapped += OnAdd;
            imgUpload.GestureRecognizers.Add(tapUploadImage);

            var tapSyncImage = new TapGestureRecognizer();
            tapSyncImage.Tapped += OnSyncItems;
            imgSync.GestureRecognizers.Add(tapSyncImage);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ErrorMessage" && viewModel.ErrorMessage != null)
            {
                DisplayAlert("Error occurred", viewModel.ErrorMessage, "Close");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //if (imagesWrap.ItemsSource == null)
            if (imagesList.ItemsSource == null)
            {
                using (var scope = new ActivityIndicatorScope(syncIndicator, true))
                {
                    if (null == viewModel.User)
                        await viewModel.GetUserAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));

                    if (null == viewModel.Album)
                        await viewModel.GetAlbumAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));

                    //await manager.SyncImagesAsync();
                    await LoadItems();
                }
            }
            App.Instance.ImageTaken += App_ImageTaken;

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            App.Instance.ImageTaken -= App_ImageTaken;
        }

        private async void App_ImageTaken(object sender, EventArgs e)
        {
            //DEBUG
            //imgPreview.Source = App.Instance.image;

            //Upload image
            using (var scope = new ActivityIndicatorScope(syncIndicator, true))
            {
                if (await viewModel.UploadImageAsync(App.Instance.ImageStream))
                {
                    await DisplayAlert("Upload succeeded", "Image uploaded and will appear in the list shortly", "Ok");
                    OnRefresh(sender, e);
                }
                else
                {
                    await DisplayAlert("Upload failed", "Image upload failed. Please try again later", "Ok");
                }
            }
        }

        private async Task LoadItems()
        {
            await viewModel.GetImagesAsync();

            if (null != viewModel.Images)
                imagesList.ItemsSource = viewModel.Images.ToList();
                //imagesWrap.ItemsSource = viewModel.Images.ToList();
        }

        public async void OnRefresh(object sender, EventArgs e)
        {
            //var list = (ListView)sender;
            var success = false;
            try
            {
                await SyncItemsAsync(false);
                success = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + ex.Message + ")", "OK");
            }
            imagesList.EndRefresh();

            if (!success)
                await DisplayAlert("Refresh Error", "Couldn't refresh data", "OK");

        }

        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedImage = e.SelectedItem as ContosoMoments.Models.Image;

            if (selectedImage != null)
            {
                var detailsView = new ImageDetailsView();
                var detailsVM = new ImageDetailsViewModel(App.MobileService, selectedImage);
                detailsVM.Album = viewModel.Album;
                detailsVM.User = viewModel.User;
                detailsView.BindingContext = detailsVM;

                await Navigation.PushAsync(detailsView);
            }

            // prevents background getting highlighted
            imagesList.SelectedItem = null;
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            App.Instance.TakePicture();
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await SyncItemsAsync(true);
        }

        private async Task SyncItemsAsync(bool showActivityIndicator)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                //await manager.SyncImagesAsync();
                await LoadItems();
            }
        }


        //private async Task AddItem(TodoItem item)
        //{
        //    await manager.SaveTaskAsync(item);
        //    await LoadItems();
        //}

        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }
}
