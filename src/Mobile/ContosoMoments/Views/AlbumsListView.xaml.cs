using ContosoMoments.Models;
using ContosoMoments.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class AlbumsListView : ContentPage, IDisposable
    {
        AlbumsListViewModel viewModel;

        public AlbumsListView()
        {
            InitializeComponent();

            viewModel = new AlbumsListViewModel(App.Instance.MobileService, App.Instance);

            BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModelPropertyChanged;
            Settings.Current.PropertyChanged += AuthTypePropertyChanged;

            viewModel.DeleteAlbumViewAction = OnDeleteAlbum;
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlbumsListViewModel.ErrorMessage) && viewModel.ErrorMessage != null) {
                DisplayAlert(viewModel.ErrorMessageTitle, viewModel.ErrorMessage, "OK");
            }
        }

        private void AuthTypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            imgAddAlbum.IsVisible = Settings.Current.AuthenticationType != Settings.AuthOption.GuestAccess;
            var settingsColumn = imgAddAlbum.IsVisible ? 3 : 4; // move the settings button to the end if the Add Album button is not shown
            Grid.SetColumn(imgSettings, settingsColumn);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            AuthTypePropertyChanged(this, new PropertyChangedEventArgs(nameof(Settings.AuthenticationType)));

            if (albumsList.ItemsSource == null) {
                await LoadItemsAsync(); // load items from the offline cache
                await SyncItemsAsync(true);
            }
        }

        private async Task LoadItemsAsync()
        {
            await viewModel.LoadItemsAsync(Settings.Current.CurrentUserId);
        }

        public async Task RefreshAsync(bool showIndicator)
        {
            try {
                await SyncItemsAsync(showIndicator);
            }
            catch (Exception ex) {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + ex.Message + ")", "OK");
            }

            albumsList.EndRefresh();
        }

        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedAlbum = e.SelectedItem as Album;

            if (selectedAlbum != null) {
                var imagesListView = new ImagesList(App.Instance, selectedAlbum);

                await Navigation.PushAsync(imagesListView);
            }

            // prevents background getting highlighted
            albumsList.SelectedItem = null;
            if (viewModel != null) {
                viewModel.ShowInputControl = false;
            }
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await RefreshAsync(false); // don't show the activity indicator, since the refresh gesture already shows it
        }

        public async void OnSettings(object sender, EventArgs e)
        {
            viewModel.ShowInputControl = false;

            var settingsView = new SettingsView(App.Instance);
            await Navigation.PushModalAsync(settingsView);
            var urlChanged = await settingsView.ShowDialog();

            if (urlChanged) {
                await App.Instance.ResetAsync();
                this.Dispose(); 
            }
        }

        private async Task SyncItemsAsync(bool showActivityIndicator)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator)) {
                viewModel.ShowInputControl = false;
                if (Utils.IsOnline() && await Utils.SiteIsOnline()) {
                    await App.Instance.SyncAlbumsAsync();

                    // should not await call to app.SyncAsync() because it should happen in the background
                    var ignore = App.Instance.SyncAsync();
                }
                else {
                    await DisplayAlert("Working Offline", "Couldn't sync data - device is offline or Web API is not available. Please try again when data connection is back", "OK");
                }

                await LoadItemsAsync();
            }
        }

        public async void OnCreateClick(object sender, EventArgs e)
        {
            var result = await viewModel.CreateOrRenameAlbum();

            if (result) {
                await RefreshAsync(true);
            }
            else {
                await DisplayAlert(viewModel.IsRename ? "Album rename error" : "Album create error", "Album name is blank", "OK");
            }
        }

        public void OnAdd(object sender, EventArgs e)
        {
            viewModel.AddImage();
        }

        public void OnCancelClick(object sender, EventArgs e)
        {
            viewModel.ShowInputControl = false;
        }

        private async void OnDeleteAlbum(Album album)
        {
            var result = await DisplayAlert("Delete album?", "Delete album and all associated images?", "Yes", "No");

            if (result) {
                await viewModel.DeleteAlbumAsync(album);
                await RefreshAsync(true);
            }
        }

        public void Dispose()
        {
            viewModel.PropertyChanged -= ViewModelPropertyChanged;
            viewModel?.Dispose();
            viewModel = null;
            Settings.Current.PropertyChanged -= AuthTypePropertyChanged;
            BindingContext = null;
        }
    }
}
