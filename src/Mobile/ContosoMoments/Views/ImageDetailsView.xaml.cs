using ContosoMoments.ViewModels;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using PCLStorage;
using System;
using System.Diagnostics;
using System.Linq;
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
                await DisplayAlert("Like sent!", "Like was sent to the image author", "OK");
            }
            catch (Exception) {
                await DisplayAlert("Error", "'Like' functionality is not available at the moment. Please try again later", "OK");
            }
        }

        public async void OnSettings(object sender, EventArgs e)
        {
            ImageDetailsViewModel vm = this.BindingContext as ImageDetailsViewModel;
            await Navigation.PushModalAsync(new SettingsView(App.Current as App));
        }

        public async void OnOpenImage(object sender, EventArgs args)
        {
            var button = (Button)sender;
            string imageSize = button.CommandParameter.ToString();

            var vm = this.BindingContext as ImageDetailsViewModel;

            IFileSyncContext context = App.Instance.MobileService.GetFileSyncContext();

            var recordFiles = await context.MobileServiceFilesClient.GetFilesAsync(App.Instance.imageTableSync.TableName, vm.Image.Id);
            var file = recordFiles.First(f => f.StoreUri.Contains(imageSize));

            if (file != null) {
                await DownloadAndDisplayImage(file, imageSize);
            }
        }

        private async Task DownloadAndDisplayImage(MobileServiceFile file, string imageSize)
        {
            try {
                var path = await FileHelper.GetLocalFilePathAsync(file.ParentId, imageSize + "-" + file.Name, App.Instance.DataFilesPath);
                await App.Instance.imageTableSync.DownloadFileAsync(file, path);
                await Navigation.PushAsync(CreateDetailsPage(path));

                // delete the file
                var fileRef = await FileSystem.Current.LocalStorage.GetFileAsync(path);
                await fileRef.DeleteAsync();
            }
            catch (Exception e) {
                // Note: we should be catching a WrappedStorageException and StorageException here, but WrappedStorageException is
                // internal in the current version of the Azure Storage library
                Debug.WriteLine("Exception downloading file: " + e.Message);
                await DisplayAlert("Error downloading image", "Error downloading, image size might not be available yet", "OK");
            }
        }

        private static MobileServiceFile GetFileReference(Models.Image image, string param)
        {
            var toDownload = new MobileServiceFile(image.File.Id, image.File.Name, image.File.TableName, image.File.ParentId) {
                StoreUri = image.File.StoreUri.Replace("lg", param)
            };

            return toDownload;
        }

        private static ContentPage CreateDetailsPage(string uri) 
        {
            var imagePage = new ContentPage {
                Content = new StackLayout() {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        new Xamarin.Forms.Image {
                            Aspect = Aspect.AspectFill,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            Source = ImageSource.FromFile(uri)
                       }
                   }
                }
            };

            return imagePage;
        }
    }
}
