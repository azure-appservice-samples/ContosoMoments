using ContosoMoments.Models;
using ContosoMoments.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public class CustomCell : ViewCell
    {
        public CustomCell()
        {
            var renameAction = new MenuItem { Text = "Rename" };
            renameAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));

            renameAction.Clicked += OnRename;

            ContextActions.Add(renameAction);
        }

        private void OnRename(object sender, EventArgs e)
        {
            Debug.WriteLine("Rename action clicked: " + ParentView);
        }
    }

    public partial class AlbumsListView : ContentPage
    {
        AlbumsListViewModel viewModel;
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
            tapNewAlbumImage.Tapped += viewModel.OnAdd;
            imgAddAlbum.GestureRecognizers.Add(tapNewAlbumImage);

            var tapSyncImage = new TapGestureRecognizer();
            tapSyncImage.Tapped += OnSyncItems;
            imgSync.GestureRecognizers.Add(tapSyncImage);

            var tapSettingsImage = new TapGestureRecognizer();
            tapSettingsImage.Tapped += OnSettings;
            imgSettings.GestureRecognizers.Add(tapSettingsImage);

            viewModel.DeleteAlbumViewAction = OnDeleteAlbum;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

            if (albumsList.ItemsSource != null) {
                // data has already been loaded, skip sync
                return;
            }

            await SyncItemsAsync(true);
        }

        private async Task LoadItemsAsync()
        {
            await viewModel.GetAlbumsAsync(_app.CurrentUserId);
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
                var imagesListView = new ImagesList(this._app);
                imagesListView.Album = selectedAlbum;

                await Navigation.PushAsync(imagesListView);
            }

            // prevents background getting highlighted
            albumsList.SelectedItem = null;
            viewModel.ShowInputControl = false;
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await RefreshAsync(false); // don't show the activity indicator, since the refresh gesture already shows it
        }

        public async void OnSettings(object sender, EventArgs e)
        {
            viewModel.ShowInputControl = false;
            await Navigation.PushModalAsync(new SettingsView(this._app));
        }

        private async Task SyncItemsAsync(bool showActivityIndicator)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator)) {
                viewModel.ShowInputControl = false;
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
    }
}
