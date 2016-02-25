using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

using PCLStorage;
using Xamarin.Forms;
using ContosoMoments.Views;

namespace ContosoMoments
{
    public class App : Application
    {
        private string ApplicationURL = @"https://donnamcontosomoments.azurewebsites.net";

        //public static string DB_LOCAL_FILENAME = "localDb-" + DateTime.Now.Ticks + ".sqlite";
        public const string LocalDbFilename = "localDb.sqlite";
        public MobileServiceClient MobileService;
        public MobileServiceUser AuthenticatedUser;

        public IMobileServiceSyncTable<Models.Album> albumTableSync;
        public IMobileServiceSyncTable<Models.Image> imageTableSync;
        public IMobileServiceSyncTable<ResizeRequest> resizeRequestSync;

        public static App Instance;
        public static object UIContext { get; set; }

        private static Object currentDownloadTaskLock = new Object();
        private static Task currentDownloadTask = Task.FromResult(0);

        public string DataFilesPath { get; set; }

        private string currentUserId;

        public string CurrentUserId
        {
            get { return currentUserId ?? Settings.Current.DefaultUserId; }
            set {
                currentUserId = value;
                Settings.Current.CurrentUserId = currentUserId;
            }
        }

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
            if (Settings.Current.MobileAppUrl == Settings.DefaultMobileAppUrl) {
                MainPage = new SettingsView(this);
            }
            else {
                await InitMobileService(Settings.Current.MobileAppUrl);
            }
        }

        internal async Task InitMobileService(string uri, bool firstStart = false)
        {
            this.ApplicationURL = uri;

            var authHandler = new AuthHandler();
            MobileService = new MobileServiceClient(ApplicationURL, new LoggingHandler(true), authHandler);
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

            if (firstStart) {
                await Utils.PopulateDefaultsAsync();

                var loginPage = new Login();
                MainPage = new NavigationPage(new AlbumsListView(this));

                await MainPage.Navigation.PushAsync(loginPage);
                await loginPage.GetResultAsync();
            }
            else {
                currentUserId = Settings.Current.CurrentUserId;
                MainPage = new NavigationPage(new AlbumsListView(this));
            }
        }

        internal async Task ResetAsync(string uri)
        {
            var platform = DependencyService.Get<IPlatform>();

            string path = Path.Combine(platform.GetRootDataPath(), LocalDbFilename);

            if (File.Exists(path)) {
                File.Delete(path);
            }

            await InitMobileService(uri, firstStart: true);
        }


        public async Task InitLocalStoreAsync(string localDbFilename)
        {
            if (!MobileService.SyncContext.IsInitialized) {
                var store = new MobileServiceSQLiteStore(localDbFilename);
                store.DefineTable<Models.Album>();
                store.DefineTable<Models.Image>();
                store.DefineTable<ResizeRequest>();

                // Initialize file sync
                MobileService.InitializeFileSyncContext(new FileSyncHandler(this), store, new FileSyncTriggerFactory(MobileService, true));

                // Uses the default conflict handler, which fails on conflict
                await MobileService.SyncContext.InitializeAsync(store, StoreTrackingOptions.NotifyLocalAndServerOperations);
            }
        }

        public async Task SyncAlbumsAsync()
        {
            await MobileService.SyncContext.PushAsync();
            await albumTableSync.PullAsync("allAlbums", albumTableSync.CreateQuery());
        }

        public async Task SyncAsync()
        {
            await imageTableSync.PushFileChangesAsync();
            await MobileService.SyncContext.PushAsync();

            await albumTableSync.PullAsync("allAlbums", albumTableSync.CreateQuery());
            await imageTableSync.PullAsync("allImages", imageTableSync.CreateQuery());
        }

        public void InitLocalTables()
        {
            try {
                albumTableSync = MobileService.GetSyncTable<Models.Album>();
                imageTableSync = MobileService.GetSyncTable<Models.Image>();
                resizeRequestSync = MobileService.GetSyncTable<ResizeRequest>();
            }
            catch (Exception ex) {
                Trace.WriteLine(ex);
            }
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

            await MobileService.EventManager.PublishAsync(new MobileServiceEvent(file.ParentId));
        }

        internal async Task<Models.Image> AddImageAsync(Models.Album album, string sourceFile)
        {
            var image = new Models.Image {
                UserId = CurrentUserId,
                AlbumId = album.AlbumId,
                UploadFormat = "Mobile Image",
                UpdatedAt = DateTime.Now
            };

            await imageTableSync.InsertAsync(image); // create a new image record

            // add image to the record
            string copiedFilePath = await FileHelper.CopyFileAsync(image.Id, sourceFile, DataFilesPath);
            string copiedFileName = Path.GetFileName(copiedFilePath);

            // add an object representing a resize request for the blob
            // it will be synced after all images have been uploaded
            await resizeRequestSync.InsertAsync(new ResizeRequest { BlobName = copiedFileName });

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
