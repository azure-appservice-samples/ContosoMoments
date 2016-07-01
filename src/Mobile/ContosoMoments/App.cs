using ContosoMoments.Views;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using PCLStorage;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments
{
    public class App : Application
    {
        public const string AppName = "ContosoMoments"; 

        public const string LocalDbFilename = "localDb.sqlite";
        private const string AllAlbumsQueryString = "allAlbums";
        private const string AllImagesQueryString = "allImages";
        public MobileServiceClient MobileService;
        public MobileServiceUser AuthenticatedUser;

        public IMobileServiceSyncTable<Models.Album> albumTableSync;
        public IMobileServiceSyncTable<Models.Image> imageTableSync;

        public static App Instance;
        public static object UIContext { get; set; }

        private static Object currentDownloadTaskLock = new Object();
        private static Task currentDownloadTask = Task.FromResult(0);

        public string DataFilesPath { get; set; }

        public App()
        {
            Instance = this;

            Label label = new Label() { Text = "Loading..." };
            label.TextColor = Color.White;
            Image img = new Image() { Source = ImageSource.FromFile("logo.png") };
            StackLayout stack = new StackLayout();
            stack.VerticalOptions = LayoutOptions.Center;
            stack.HorizontalOptions = LayoutOptions.Center;
            stack.Orientation = StackOrientation.Vertical;
            stack.Children.Add(img);
            stack.Children.Add(label);
            ContentPage page = new ContentPage();
            page.BackgroundColor = Color.FromHex("#8C0A4B");
            page.Content = stack;
            MainPage = page;
        }

        protected override async void OnStart()
        {
            bool firstStart = Settings.IsFirstStart();
            await InitMobileService(showSettingsPage: firstStart, showLoginDialog: firstStart);
        }

        internal async Task InitMobileService(bool showSettingsPage, bool showLoginDialog)
        {
            if (showSettingsPage) {
                var settingsView = new SettingsView(this);
                await MainPage.Navigation.PushModalAsync(settingsView);
                await settingsView.ShowDialog();
            }

            var authHandler = new AuthHandler();
            MobileService = 
                new MobileServiceClient(Settings.Current.MobileAppUrl, new LoggingHandler(true), authHandler);

            authHandler.Client = MobileService;
            AuthenticatedUser = MobileService.CurrentUser;

            await InitLocalStoreAsync(LocalDbFilename);
            InitLocalTables();

            IPlatform platform = DependencyService.Get<IPlatform>();
            DataFilesPath = await platform.GetDataFilesPath();

#if __DROID__ && PUSH
            Droid.GcmService.RegisterWithMobilePushNotifications();
#elif __IOS__ && PUSH
           iOS.AppDelegate.IsAfterLogin = true;
           await iOS.AppDelegate.RegisterWithMobilePushNotifications();
#elif __WP__ && PUSH
           ContosoMoments.WinPhone.App.AcquirePushChannel(App.Instance.MobileService);
#endif

            if (showLoginDialog) {
                await Utils.PopulateDefaultsAsync();

                await DoLoginAsync();

                Debug.WriteLine("*** DoLoginAsync complete");

                MainPage = new NavigationPage(new AlbumsListView());
            }
            else {
                // user has already chosen an authentication type, so re-authenticate
                await AuthHandler.DoLoginAsync(Settings.Current.AuthenticationType);

                MainPage = new NavigationPage(new AlbumsListView());
            }
        }

        internal async Task ResetAsync()
        {
            await PurgeDataAsync();
            MobileService.Dispose();

            await InitMobileService(showSettingsPage: false, showLoginDialog: true);
        }

        private async Task DoLoginAsync()
        {
            var loginPage = new Login();
            await MainPage.Navigation.PushModalAsync(loginPage);
            Settings.Current.AuthenticationType = await loginPage.GetResultAsync();

            await SyncAsync(notify: true);
        }

        internal async Task LogoutAsync()
        {
            DependencyService.Get<IPlatform>().LogEvent("Logout" + Settings.Current.AuthenticationType);

            await PurgeDataAsync();
            await DoLoginAsync();
        }

        private async Task PurgeDataAsync()
        {
            var platform = DependencyService.Get<IPlatform>();
            await platform.LogoutAsync();

            await imageTableSync.PurgeAsync(AllImagesQueryString, null, true, CancellationToken.None);
            await albumTableSync.PurgeAsync(AllAlbumsQueryString, null, true, CancellationToken.None);

            // delete downloaded files
            await FileHelper.DeleteLocalPathAsync(await platform.GetDataFilesPath());

            Settings.Current.AuthenticationType = Settings.AuthOption.GuestAccess;
            Settings.Current.CurrentUserId = Settings.Current.DefaultUserId;
        }

        public async Task InitLocalStoreAsync(string localDbFilename)
        {
            if (!MobileService.SyncContext.IsInitialized) {
                var store = new MobileServiceSQLiteStore(localDbFilename);
                store.DefineTable<Models.Album>();
                store.DefineTable<Models.Image>();

                // Initialize file sync
                MobileService.InitializeFileSyncContext(new FileSyncHandler(this), store, new FileSyncTriggerFactory(MobileService, true));

                // Uses the default conflict handler, which fails on conflict
                await MobileService.SyncContext.InitializeAsync(store, StoreTrackingOptions.NotifyLocalAndServerOperations);
            }
        }

        public async Task SyncAlbumsAsync()
        {
            await MobileService.SyncContext.PushAsync();
            await albumTableSync.PullAsync(AllAlbumsQueryString, albumTableSync.CreateQuery());
        }

        public async Task SyncAsync(bool notify = false)
        {
            await imageTableSync.PushFileChangesAsync();
            await MobileService.SyncContext.PushAsync();

            await albumTableSync.PullAsync(AllAlbumsQueryString, albumTableSync.CreateQuery());
            await imageTableSync.PullAsync(AllImagesQueryString, imageTableSync.CreateQuery());

            if (notify) {
                await MobileService.EventManager.PublishAsync(SyncCompletedEvent.Instance);
            }
        }

        public void InitLocalTables()
        {
            albumTableSync = MobileService.GetSyncTable<Models.Album>();
            imageTableSync = MobileService.GetSyncTable<Models.Image>();
        }

        internal Task DownloadFileAsync(MobileServiceFile file)
        {
            // should only download one file at a time, since it's possible to get duplicate notifications for the same file
            // ContinueWith is used along with Wait() so that only one thread downloads at a time
            lock (currentDownloadTaskLock) {
                return currentDownloadTask =
                    currentDownloadTask.ContinueWith(x => DoFileDownloadAsync(file)).Unwrap();
            }
        }

        private async Task DoFileDownloadAsync(MobileServiceFile file)
        {
            Debug.WriteLine("Starting file download - " + file.Name);

            IPlatform platform = DependencyService.Get<IPlatform>();
            var path = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name, DataFilesPath);
            var tempPath = Path.ChangeExtension(path, ".temp");

            await platform.DownloadFileAsync(imageTableSync, file, tempPath);

            var fileRef = await FileSystem.Current.LocalStorage.GetFileAsync(tempPath);
            await fileRef.RenameAsync(path, NameCollisionOption.ReplaceExisting);
            Debug.WriteLine("Renamed file to - " + path);

            await MobileService.EventManager.PublishAsync(new ImageDownloadEvent(file.ParentId));
        }

        internal async Task<Models.Image> AddImageAsync(Models.Album album, string sourceFile)
        {
            var image = new Models.Image {
                UserId = Settings.Current.CurrentUserId,
                AlbumId = album.AlbumId,
                UploadFormat = "Mobile Image",
                UpdatedAt = DateTime.Now
            };

            await imageTableSync.InsertAsync(image); // create a new image record

            // add image to the record
            string copiedFilePath = await FileHelper.CopyFileAsync(image.Id, sourceFile, DataFilesPath);
            string copiedFileName = Path.GetFileName(copiedFilePath);

            var file = await imageTableSync.AddFileAsync(image, copiedFileName);
            image.File = file;

            return image;
        }

        internal async Task DeleteImageAsync(Models.Image item, MobileServiceFile file)
        {
            await imageTableSync.DeleteFileAsync(file);
        }
    }
}
