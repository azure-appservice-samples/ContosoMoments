using ContosoMoments.Settings;
using ContosoMoments.Views;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices.Files;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using PCLStorage;

namespace ContosoMoments
{
    public class ResizeRequest
    {
        public string Id { get; set; }
        public string BlobName { get; set; }
    }

    public class App : Application
    {
        public static string ApplicationURL = @"https://donnamcontosomoments.azurewebsites.net";

        //public static string DB_LOCAL_FILENAME = "localDb-" + DateTime.Now.Ticks + ".sqlite";
        public static string DB_LOCAL_FILENAME = "localDb.sqlite";
        public static MobileServiceClient MobileService;
        public static MobileServiceUser AuthenticatedUser;

        public IMobileServiceSyncTable<Models.Album> albumTableSync;
        public IMobileServiceSyncTable<Models.User> userTableSync;
        public IMobileServiceSyncTable<Models.Image> imageTableSync;
        public IMobileServiceSyncTable<ResizeRequest> resizeRequestSync;

        public static App Instance;
        public static object UIContext { get; set; }

        private static Object currentDownloadTaskLock = new Object();
        private static Task currentDownloadTask = Task.FromResult(0);

        public App()
        {
            Instance = this;

            Label label = new Label() { Text = "Loading..." };
            label.TextColor = Color.White;
            Image img = new Image()
            {
                Source = Device.OnPlatform(
                    iOS: ImageSource.FromFile("Assets/logo.png"),
                    Android: ImageSource.FromFile("logo.png"),
                    WinPhone: ImageSource.FromFile("Assets/logo.png"))
            };
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
            bool isAuthRequred = false;

            var authHandler = new AuthHandler(DependencyService.Get<Models.IMobileClient>());
            MobileService = new MobileServiceClient(ApplicationURL, new LoggingHandler(true), authHandler);
            authHandler.Client = MobileService;
            AuthenticatedUser = MobileService.CurrentUser;

            if (AppSettings.Current.GetValueOrDefault<bool>("ConfigChanged"))
            {
                ClearLocalStorage(DB_LOCAL_FILENAME);
                AppSettings.Current.AddOrUpdateValue<bool>("ConfigChanged", false);
            }

            await InitLocalStoreAsync(DB_LOCAL_FILENAME);
            InitLocalTables();

            if (isAuthRequred && AuthenticatedUser == null)
            {
                MainPage = new NavigationPage(new Login());
            }
            else
            {
#if __DROID__ && PUSH
                Droid.GcmService.RegisterWithMobilePushNotifications();
#elif __IOS__ && PUSH
                iOS.AppDelegate.IsAfterLogin = true;
                await iOS.AppDelegate.RegisterWithMobilePushNotifications();
#elif __WP__ && PUSH
                ContosoMoments.WinPhone.App.AcquirePushChannel(App.MobileService);
#endif
                MainPage = new NavigationPage(new AlbumsListView(this));
            }
        }

        private void ClearLocalStorage(string localDbFilename)
        {
            string dataDir = DependencyService.Get<IPlatform>().GetDataPathAsync();
            string path = Path.Combine(dataDir, localDbFilename);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public async Task InitLocalStoreAsync(string localDbFilename)
        {
            if (!MobileService.SyncContext.IsInitialized)
            {
                var store = new MobileServiceSQLiteStore(localDbFilename);
                store.DefineTable<Models.User>();
                store.DefineTable<Models.Album>();
                store.DefineTable<Models.Image>();
                store.DefineTable<ResizeRequest>();

                // Initialize file sync
                MobileService.InitializeFileSyncContext(new FileSyncHandler(this), store, new FileSyncTriggerFactory(MobileService, true));

                // Uses the default conflict handler, which fails on conflict
                await MobileService.SyncContext.InitializeAsync(store, StoreTrackingOptions.NotifyLocalAndServerOperations);
            }
        }

        public async Task SyncAsync()
        {
            Debug.WriteLine($"Pending resize requests: { (await resizeRequestSync.CreateQuery().ToListAsync()).Count }");

            await imageTableSync.PushFileChangesAsync();
            await MobileService.SyncContext.PushAsync();
            await userTableSync.PullAsync("allUsers", userTableSync.CreateQuery()); // query ID is used for incremental sync
            await albumTableSync.PullAsync("allAlbums", albumTableSync.CreateQuery()); 
            await imageTableSync.PullAsync("allImages", imageTableSync.CreateQuery());

            Debug.WriteLine($"Pending resize requests: { (await resizeRequestSync.CreateQuery().ToListAsync()).Count }");
        }

        public void InitLocalTables()
        {
            try
            {
                userTableSync = MobileService.GetSyncTable<Models.User>(); // offline sync
                albumTableSync = MobileService.GetSyncTable<Models.Album>(); // offline sync
                imageTableSync = MobileService.GetSyncTable<Models.Image>(); // offline sync
                resizeRequestSync = MobileService.GetSyncTable<ResizeRequest>();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        internal Task DownloadFileAsync(MobileServiceFile file)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();

            lock (currentDownloadTaskLock) {
                return currentDownloadTask =
                    currentDownloadTask.ContinueWith(
                    x => {
                        var path = FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name).Result;

                        Debug.WriteLine("Starting file download - " + file.Name);
                        platform.DownloadFileAsync(imageTableSync, file, file.Name).Wait();
                        Debug.WriteLine("Completed file download - " + file.Name);
                    }
                );
            }
        }

        internal async Task<MobileServiceFile> AddImage(Models.User user, Models.Album album, string sourceFile)
        {
            var image = new Models.Image {
                UserId = user.UserId.ToString(),
                AlbumId = album.AlbumId,
                UploadFormat = "Mobile Image",
                FileName = Guid.NewGuid().ToString()
            };

            await imageTableSync.InsertAsync(image); // create a new image record

            // add image to the record
            string copiedFilePath = await FileHelper.CopyFileAsync(image.Id, sourceFile);
            string copiedFileName = Path.GetFileName(copiedFilePath);

            // add an object representing a resize request for the blob
            // it will be synced after all images have been uploaded
            await resizeRequestSync.InsertAsync(new ResizeRequest { BlobName = copiedFileName });

            return await imageTableSync.AddFileAsync(image, copiedFileName);
        }

        internal async Task DeleteImage(Models.Image item, MobileServiceFile file)
        {
            await imageTableSync.DeleteFileAsync(file);
        }
    }
}
