using ContosoMoments.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using System.ComponentModel;

namespace ContosoMoments.Views
{
    public partial class AlbumsListView : ContentPage
    {
        AlbumsListViewModel viewModel;
        bool? isNew = null;
        ContosoMoments.Models.Album editedAlbum = null;
        private App _app;

        public AlbumsListView(App app)
        {
            InitializeComponent();
            this._app = app;

            viewModel = new AlbumsListViewModel(App.Instance.MobileService, app);

            BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            Settings.Current.PropertyChanged += AuthTypePropertyChanged;

            // new album creation is only allowed in authenticated mode
            var tapNewAlbumImage = new TapGestureRecognizer();
            tapNewAlbumImage.Tapped += OnAdd;
            imgAddAlbum.GestureRecognizers.Add(tapNewAlbumImage);

            var tapSyncImage = new TapGestureRecognizer();
            tapSyncImage.Tapped += OnSyncItems;
            imgSync.GestureRecognizers.Add(tapSyncImage);

            var tapSettingsImage = new TapGestureRecognizer();
            tapSettingsImage.Tapped += OnSettings;
            imgSettings.GestureRecognizers.Add(tapSettingsImage);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlbumsListViewModel.ErrorMessage) && viewModel.ErrorMessage != null) {
                DisplayAlert("Error occurred", viewModel.ErrorMessage, "Close");
            }
        }

        private void AuthTypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            imgAddAlbum.IsVisible = Settings.Current.AuthenticationType != Settings.AuthOption.GuestAccess;

            if (!imgAddAlbum.IsVisible)
               Grid.SetColumn(imgSettings, 4);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            AuthTypePropertyChanged(this, new PropertyChangedEventArgs(nameof(Settings.AuthenticationType)));

            if (albumsList.ItemsSource != null) {
                return;
            }

            using (var scope = new ActivityIndicatorScope(syncIndicator, true)) {

                if (Utils.IsOnline() && await Utils.SiteIsOnline()) {

                    await _app.SyncAlbumsAsync();

#pragma warning disable CS4014  // should not await call to _app.SyncAsync() because it should happen in the background
                    _app.SyncAsync();
#pragma warning restore CS4014
                }
                else
                    await DisplayAlert("Working Offline", "Couldn't sync data - device is offline or Web API is not available. Using local data. Please try again when data connection is back", "OK");

                await LoadItemsAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private async Task LoadItemsAsync()
        {
            await viewModel.GetAlbumsAsync(_app.CurrentUserId);

            if (viewModel.Albums != null) {
                albumsList.ItemsSource = null;
                albumsList.ItemsSource = viewModel.Albums.ToList();
            }
        }

        public async void OnRefresh(object sender, EventArgs e)
        {
            var success = false;
            try {
                await SyncItemsAsync(true);
                success = true;
            }
            catch (Exception ex) {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + ex.Message + ")", "OK");
            }
            albumsList.EndRefresh();

            if (!success)
                await DisplayAlert("Refresh Error", "Couldn't refresh data", "OK");

        }

        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedAlbum = e.SelectedItem as ContosoMoments.Models.Album;

            if (selectedAlbum != null) {
                var imagesListView = new ImagesList(this._app);
                imagesListView.Album = selectedAlbum;

                await Navigation.PushAsync(imagesListView);
            }

            // prevents background getting highlighted
            albumsList.SelectedItem = null;

            HideAndCleanupInput();
        }

        public void OnCancelClick(object sender, EventArgs e)
        {
            HideAndCleanupInput();
        }

        public async void OnCreateClick(object sender, EventArgs e)
        {
            string errorMessage = "";

            if (entAlbumName.Text != null && entAlbumName.Text.Length > 0) {
                try {

                    if (isNew.Value) {
                        errorMessage = "Couldn't create new album. Please try again later.";

                        await viewModel.AddNewAlbumAsync(entAlbumName.Text);
                    }
                    else {
                        errorMessage = "Couldn't rename album. Please try again later.";

                        editedAlbum.AlbumName = entAlbumName.Text;
                        await viewModel.UpdateAlbumAsync(editedAlbum);
                    }

                    HideAndCleanupInput();
                    OnRefresh(sender, e);
                }

                catch (Exception) {
                    await DisplayAlert("Album error", errorMessage, "OK");
                }
            }
            else {
                await DisplayAlert("Album creation error", "New album name is empty. Please enter new album name and try again.", "OK");
            }
        }

        private void HideAndCleanupInput()
        {
            btnCancel.IsVisible = false;
            entAlbumName.Text = "";
            isNew = null;
            editedAlbum = null;
            grdInput.IsVisible = false;

        }

        public async void OnDelete(object sender, EventArgs e)
        {
            var selectedAlbum = (sender as MenuItem).BindingContext as ContosoMoments.Models.Album;

            if (selectedAlbum != null) {
                if (!selectedAlbum.IsDefault) {
                    var res = await DisplayAlert("Delete album?", "Delete album and all associated images?", "Yes", "No");

                    if (res) {
                        try {
                            await viewModel.DeleteAlbumAsync(selectedAlbum);
                            HideAndCleanupInput();
                            OnRefresh(sender, e);
                        }
                        catch (Exception) {
                            await DisplayAlert("Delete error", "Couldn't delete the album. Please try again later.", "OK");
                        }
                    }
                }
                else
                    await DisplayAlert("Delete album", "Can't delete default album", "OK");
            }
        }

        public async void OnRename(object sender, EventArgs e)
        {
            isNew = false;

            var selectedAlbum = (sender as MenuItem).BindingContext as ContosoMoments.Models.Album;

            if (null != selectedAlbum) {
                if (!selectedAlbum.IsDefault) {
                    editedAlbum = selectedAlbum;
                    entAlbumName.Text = selectedAlbum.AlbumName;
                    grdInput.IsVisible = true;
                    btnCancel.IsVisible = true;
                    btnUpdate.Text = "Update";
                }
                else
                    await DisplayAlert("Rename album", "Can't rename default album.", "OK");
            }
        }

        public void OnAdd(object sender, EventArgs e)
        {
            isNew = true;
            entAlbumName.Text = "";
            btnUpdate.Text = "Create";
            btnCancel.IsVisible = false;
            grdInput.IsVisible = !grdInput.IsVisible;
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await SyncItemsAsync(true);
        }

        public async void OnSettings(object sender, EventArgs e)
        {
            HideAndCleanupInput();
            await Navigation.PushModalAsync(new SettingsView(this._app));
        }

        private async Task SyncItemsAsync(bool showActivityIndicator)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator)) {
                HideAndCleanupInput();
                if (Utils.IsOnline() && await Utils.SiteIsOnline()) {
                    await _app.SyncAlbumsAsync();

#pragma warning disable CS4014  // should not await call to _app.SyncAsync() because it should happen in the background
                    _app.SyncAsync();
#pragma warning restore CS4014
                }
                else {
                    await DisplayAlert("Working Offline", "Couldn't sync data - device is offline or Web API is not available. Please try again when data connection is back", "OK");
                }

                await LoadItemsAsync();
            }
        }
    }
}
