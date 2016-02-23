using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace ContosoMoments.Helpers
{
    class Settings
    {
        public static string DefaultUserId
        {
            get { return AppSettings.GetValueOrDefault<string>(DefaultUserIdKey, UserIdDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DefaultUserIdKey, value); }
        }

        public static string DefaultAlbumId
        {
            get { return AppSettings.GetValueOrDefault<string>(DefaultAlbumIdKey, AlbumIdDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DefaultAlbumIdKey, value); }
        }

        public static string MobileAppUrl
        {
            get { return AppSettings.GetValueOrDefault<string>(MobileAppUrlKey, DefaultMobileAppUrl); }
            set { AppSettings.AddOrUpdateValue<string>(MobileAppUrlKey, value); }
        }
        
        private const string DefaultUserIdKey = nameof(DefaultUserIdKey);
        public const string UserIdDefault = "";

        private const string DefaultAlbumIdKey = nameof(DefaultAlbumIdKey);
        public const string AlbumIdDefault = "";

        private const string MobileAppUrlKey = nameof(MobileAppUrlKey);
        public const string DefaultMobileAppUrl = "";

        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }
    }
}
