using ContosoMoments.Settings;
using ContosoMoments.Views;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices.Files;
using System.Diagnostics;

namespace ContosoMoments
{
    public class App : Application
    {
        public static string ApplicationURL = @"https://donnamcontosomoments.azurewebsites.net";

        public const string DB_LOCAL_FILENAME = "localDb.sqlite";
        public static MobileServiceClient MobileService;
        public static MobileServiceUser AuthenticatedUser;

        public IMobileServiceSyncTable<Models.Album> albumTableSync;
        public IMobileServiceSyncTable<Models.User> userTableSync;
        public IMobileServiceSyncTable<Models.Image> imageTableSync;

        public static App Instance;
        //DEBUG
        //public ImageSource image = null;
        public Stream ImageStream = null;
        public event Action ShouldTakePicture = () => { };
        public event EventHandler ImageTaken;

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
            //Constants.GatewayURL = AppSettings.Current.GetValueOrDefault<string>("GatewayURL");
            // TODO: auth
            bool isAuthRequred = false; 

            MobileService = new MobileServiceClient(ApplicationURL, new LoggingHandler(true));
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

                MainPage = new NavigationPage(new AlbumsListView());
            }
        }

        private void ClearLocalStorage(string localDbFilename)
        {
#if !__WP__
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), localDbFilename);
#else
            string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, localDbFilename);
#endif

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

#if !__WP__
        public async void ShowCapturedImage(string filepath)
        {
            if (null != filepath)
            {
                FileStream fs = new FileStream(filepath, FileMode.Open);
                if (fs.CanRead)
                {
                    byte[] buffer = new byte[fs.Length];
                    await fs.ReadAsync(buffer, 0, (int)fs.Length);

                    ImageStream = new MemoryStream(buffer);
                }

                //DEBUG
                //image = ImageSource.FromFile(filepath);

                if (null != ImageTaken)
                    ImageTaken(this, new EventArgs());
            }
            else
            {
                ImageStream = null;
                if (null != ImageTaken)
                    ImageTaken(this, new EventArgs());
            }
        }
#elif __WP__
        public async void ShowCapturedImage(Stream stream)
        {
            //DEBUG
            //image = ImageSource.FromStream(() => stream);
            if (null != stream)
            {
                byte[] bytes = new byte[(int)stream.Length];
                await stream.ReadAsync(bytes, 0, (int)stream.Length);

                ImageStream = new MemoryStream(bytes);

                if (null != ImageTaken)
                    ImageTaken(this, new EventArgs());
            }
            else
            {
                ImageStream = null;
                if (null != ImageTaken)
                    ImageTaken(this, new EventArgs());
            }
        }
#endif

        public void TakePicture()
        {
            ShouldTakePicture();
        }

        public async Task InitLocalStoreAsync(string localDbFilename)
        {
            if (!MobileService.SyncContext.IsInitialized)
            {
                // new code to initialize the SQLite store
#if !__WP__
                string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), localDbFilename);
#else
                string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, localDbFilename);
#endif

                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }

                var store = new MobileServiceSQLiteStore(path);
                store.DefineTable<Models.User>();
                store.DefineTable<Models.Album>();
                store.DefineTable<Models.Image>();

                // Initialize file sync
                MobileService.InitializeFileSyncContext(new FileSyncHandler(this), store, new FileSyncTriggerFactory(MobileService, true));

                // Uses the default conflict handler, which fails on conflict
                await MobileService.SyncContext.InitializeAsync(store, StoreTrackingOptions.NotifyLocalAndServerOperations);
            }
        }

        public async Task SyncAsync()
        {
            await imageTableSync.PushFileChangesAsync();
            await MobileService.SyncContext.PushAsync();
            await userTableSync.PullAsync("allUsers", userTableSync.CreateQuery()); // query ID is used for incremental sync
            await albumTableSync.PullAsync("allAlbums", albumTableSync.CreateQuery()); // query ID is used for incremental sync
            await imageTableSync.PullAsync("allImages", imageTableSync.CreateQuery()); // query ID is used for incremental sync
        }

        public void InitLocalTables()
        {
            try
            {
                userTableSync = MobileService.GetSyncTable<Models.User>(); // offline sync
                albumTableSync = MobileService.GetSyncTable<Models.Album>(); // offline sync
                imageTableSync = MobileService.GetSyncTable<Models.Image>(); // offline sync
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        internal async Task DownloadFileAsync(MobileServiceFile file)
        {
            var todoItem = await imageTableSync.LookupAsync(file.ParentId);
            IPlatform platform = DependencyService.Get<IPlatform>();

            string filePath = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name);
            await platform.DownloadFileAsync(imageTableSync, file, filePath);
        }

        internal async Task<MobileServiceFile> AddImage(Models.Image image, Stream imageStream)
        {
            string targetPath = await FileHelper.SaveStreamAsync(itemId: image.Id, filename: image.Id, sourceStream: imageStream);
            return await imageTableSync.AddFileAsync(image, Path.GetFileName(targetPath));
        }

        internal async Task DeleteImage(Image todoItem, MobileServiceFile file)
        {
            await imageTableSync.DeleteFileAsync(file);
        }

        internal async Task<IEnumerable<MobileServiceFile>> GetImageFilesAsync(Models.Image image)
        {
            return await imageTableSync.GetFilesAsync(image);
        }
    }
}
