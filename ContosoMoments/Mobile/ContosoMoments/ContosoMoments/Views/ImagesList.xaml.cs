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

        public ImagesList ()
		{
			InitializeComponent ();

            if (Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone)
            {
                syncButton.IsVisible = true;
            }

            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //if (imagesList.ItemsSource == null)
            //{
            //    await SyncItemsAsync(true);
            //    await LoadItems();
            //}

            await viewModel.GetImagesAsync();

        }

        private async Task LoadItems()
        {
            //IEnumerable<Models.Image> items = await manager.GetImagesAsync();

            //imagesList.ItemsSource = items.Select(i => new ImagesListViewModel(i, this.manager));
        }


        public async void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            var success = false;
            try
            {
                await SyncItemsAsync(false);
                success = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert ("Refresh Error", "Couldn't refresh data ("+ex.Message+")", "OK");
            }
            list.EndRefresh();

            if (!success)
                await DisplayAlert("Refresh Error", "Couldn't refresh data", "OK");

        }

        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var todo = e.SelectedItem as ImagesListViewModel;

            if (todo != null)
            {
                var detailsView = new ImageDetailsView();
                detailsView.BindingContext = todo;

                await Navigation.PushAsync(detailsView);
            }

            // prevents background getting highlighted
            imagesList.SelectedItem = null;
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            
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
                //await LoadItems();
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
