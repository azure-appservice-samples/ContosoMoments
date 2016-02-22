using ContosoMoments.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class AlbumsListView : ContentPage
    {
        AlbumsListViewModel viewModel = new AlbumsListViewModel(App.MobileService);
        bool? isNew = null;
        ContosoMoments.Models.Album editedAlbum = null;

        public AlbumsListView()
        {
            InitializeComponent();

            BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

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
            if (e.PropertyName == "ErrorMessage" && viewModel.ErrorMessage != null)
            {
                DisplayAlert("Error occurred", viewModel.ErrorMessage, "Close");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (albumsList.ItemsSource == null)
            {
                using (var scope = new ActivityIndicatorScope(syncIndicator, true))
                {
                    string userId = "11111111-1111-1111-1111-111111111111";
                    if (Utils.IsOnline() && await Utils.SiteIsOnline())
                    {
                        //Call user custom controller:
                        //controller to check user and add if new. Will return user ID anyway.
                        //must be called prior to sync!!!
                        userId = await App.MobileService.InvokeApiAsync<string>("ManageUser", System.Net.Http.HttpMethod.Get, null);
#if !__WP__ || (__WP__ && DEBUG)
                        viewModel.CheckUpdateNotificationRegistrationAsync(userId);
#endif
                        await (App.Current as App).SyncAsync();
                    }
                    else
                        await DisplayAlert("Working Offline", "Couldn't sync data - device is offline or Web API is not available. Using local data. Please try again when data connection is back", "OK");

                    if (null == viewModel.User)
                        await viewModel.GetUserAsync(Guid.Parse(userId));

                    await LoadItems();
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private async Task LoadItems()
        {
            await viewModel.GetAlbumsAsync();

            if (null != viewModel.Albums)
            {
                albumsList.ItemsSource = null;
                albumsList.ItemsSource = viewModel.Albums.ToList();
            }
        }

        public async void OnRefresh(object sender, EventArgs e)
        {
            var success = false;
            try
            {
                await SyncItemsAsync(true);
                success = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + ex.Message + ")", "OK");
            }
            albumsList.EndRefresh();

            if (!success)
                await DisplayAlert("Refresh Error", "Couldn't refresh data", "OK");

        }

        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedAlbum = e.SelectedItem as ContosoMoments.Models.Album;

            if (selectedAlbum != null)
            {
                var imagesListView = new ImagesList();
                imagesListView.User = viewModel.User;
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
            if (null != entAlbumName.Text)
            {
                if (entAlbumName.Text.Length > 0)
                {
                    if (isNew.Value)
                    {
                        bool res = await viewModel.AddNewAlbumAsync(entAlbumName.Text);

                        if (res)
                        {
                            await DisplayAlert("Success", "Album created successfully and will appear in the list shortly", "OK");
                            HideAndCleanupInput();
                            OnRefresh(sender, e);
                        }
                        else
                            await DisplayAlert("Album creation error", "Couldn't create new album. Please try again later.", "OK");
                    }
                    else if (!isNew.Value)
                    {
                        editedAlbum.AlbumName = entAlbumName.Text;
                        bool res = await viewModel.UpdateAlbumAsync(editedAlbum);

                        if (res)
                        {
                            await DisplayAlert("Success", "Album renamed successfully and will appear in the list shortly", "OK");
                            HideAndCleanupInput();
                            OnRefresh(sender, e);
                        }
                        else
                            await DisplayAlert("Album update error", "Couldn't rename album. Please try again later.", "OK");
                    }
                }
                else
                    await DisplayAlert("Album creation error", "New album name is empty. Please enter new album name and try again later.", "OK");
            }
            else
                await DisplayAlert("Album creation error", "New album name is empty. Please enter new album name and try again later.", "OK");
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

            if (null != selectedAlbum)
            {
                if (!selectedAlbum.IsDefault)
                {
                    var res = await DisplayAlert("Delete album?", "Delete album and all associated images?", "Yes", "No");

                    if (res)
                    {
                        res = await viewModel.DeleteAlbumAsync(selectedAlbum);

                        if (res)
                        {
                            await DisplayAlert("Success", "Album deleted successfully", "OK");
                            HideAndCleanupInput();
                            OnRefresh(sender, e);
                        }
                        else
                            await DisplayAlert("Delete error", "Couldn't delete the album. Please try again later.", "OK");
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

            if (null != selectedAlbum)
            {
                if (!selectedAlbum.IsDefault)
                {
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

        public async void OnAdd(object sender, EventArgs e)
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
            await Navigation.PushModalAsync(new SettingView());
        }

        private async Task SyncItemsAsync(bool showActivityIndicator)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                HideAndCleanupInput();
                if (Utils.IsOnline() && await Utils.SiteIsOnline())
                {
                    await (App.Current as App).SyncAsync();
                }
                else
                {
                    await DisplayAlert("Working Offline", "Couldn't sync data - device is offline or Web API is not available. Please try again when data connection is back", "OK");
                }

                await LoadItems();
            }
        }

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
