﻿using ContosoMoments.Models;
using ContosoMoments.ViewModels;
using System;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class ImagesList : ContentPage
    {
        private Album album;

        private ImagesListViewModel viewModel;

        public ImagesList(App app, Album album)
        {
            InitializeComponent();

            this.album = album;

            // allow image upload to the public album with the default service only if logged in with AAD
            bool showImageUpload =
                !album.IsDefault ||
                (!Settings.IsDefaultServiceUrl() ||
                 Settings.Current.AuthenticationType == Settings.AuthOption.ActiveDirectory);

            imgUpload.IsVisible = showImageUpload;

            viewModel = new ImagesListViewModel(App.Instance.MobileService, App.Instance);
            BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.DeleteImageViewAction = OnDelete;           
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ErrorMessage" && viewModel.ErrorMessage != null) {
                DisplayAlert("Error occurred", viewModel.ErrorMessage, "Close");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (imagesList.ItemsSource == null) {
                using (var scope = new ActivityIndicatorScope(syncIndicator, true)) {
                    viewModel.Album = album;
                    await LoadItemsAsync();
                }
            }
        }

        private async void OnAddImage(object sender, EventArgs e)
        {
            DependencyService.Get<IPlatform>().LogEvent("AddImage");

            using (var scope = new ActivityIndicatorScope(syncIndicator, true)) {
                try {
                    IPlatform platform = DependencyService.Get<IPlatform>();
                    string sourceImagePath = await platform.TakePhotoAsync(App.UIContext);

                    if (sourceImagePath != null) {
                        var image = await App.Instance.AddImageAsync(viewModel.Album, sourceImagePath);

                        viewModel.Images.Add(image); // add image, item will appear and image will upload asynchronously
                        await SyncItemsAsync(true, refreshView: false);
                    }

                }
                catch (Exception) {
                    await DisplayAlert("Upload failed", "Image upload failed. Please try again later", "Ok");
                }
            }
        }

        private async Task LoadItemsAsync()
        {
            await viewModel.LoadImagesAsync(viewModel.Album.AlbumId);
        }

        public async Task RefreshAsync()
        {
            try {
                await SyncItemsAsync(true, refreshView: true);
            }
            catch (Exception) {
                await DisplayAlert("Refresh Error", "Couldn't refresh data", "OK");
            }

            imagesList.EndRefresh();
        }

        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedImage = e.SelectedItem as ContosoMoments.Models.Image;

            if (selectedImage != null) {
                var detailsVM = new ImageDetailsViewModel(App.Instance.MobileService, selectedImage);
                var detailsView = new ImageDetailsView();
                detailsVM.Album = viewModel.Album;
                detailsView.BindingContext = detailsVM;

                await Navigation.PushAsync(detailsView);
            }

            // prevents background getting highlighted
            imagesList.SelectedItem = null;
        }
            
        public async void OnSyncItems(object sender, EventArgs e)
        {
            await SyncItemsAsync(false, refreshView: true);
            imagesList.EndRefresh();
        }

        private async Task SyncItemsAsync(bool showActivityIndicator, bool refreshView)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator)) {
                if (Utils.IsOnline() && await Utils.SiteIsOnline()) {
                    await App.Instance.SyncAsync();
                }
                else {
                    await DisplayAlert("Working Offline", "Couldn't sync data - device may be offline", "OK");
                }

                if (refreshView) {
                    await LoadItemsAsync();
                }
            }
        }

        public async void OnDelete(Models.Image image)
        {
            var result = await DisplayAlert("Delete image?", "Delete selected image?", "Yes", "No");

            if (result) {
                DependencyService.Get<IPlatform>().LogEvent("DeleteImage");

                try {
                    await viewModel.DeleteImageAsync(image);
                    await RefreshAsync();
                }
                catch (Exception) {
                    await DisplayAlert("Delete error", "Could not delete the image", "OK");
                }
            }
        }
    }
}
