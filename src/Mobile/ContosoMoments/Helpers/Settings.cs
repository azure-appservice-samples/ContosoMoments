using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace ContosoMoments.Helpers
{
    public class Settings
    {
        public enum AuthOption
        {
            GuestAccess, Facebook, ActiveDirectory
        }

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

        public static AuthOption AuthenticationType
        {
            get { return AppSettings.GetValueOrDefault<AuthOption>(AuthenticationTypeKey, DefaultAuthType); }
            set { AppSettings.AddOrUpdateValue<AuthOption>(AuthenticationTypeKey, value); }
        }

        private const string DefaultUserIdKey = nameof(DefaultUserIdKey);
        public const string UserIdDefault = "";

        private const string DefaultAlbumIdKey = nameof(DefaultAlbumIdKey);
        public const string AlbumIdDefault = "";

        private const string MobileAppUrlKey = nameof(MobileAppUrlKey);
        public const string DefaultMobileAppUrl = "";

        private const string AuthenticationTypeKey = nameof(AuthenticationTypeKey);
        public const AuthOption DefaultAuthType = AuthOption.GuestAccess;

        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }
    }
}
