using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContosoMoments
{
    public class Settings : INotifyPropertyChanged
    {
        static Settings settings;
        public static Settings Current
        {
            get { return settings ?? (settings = new Settings()); }
        }

        public static bool IsFirstStart()
        {
            return Current.DefaultUserId == UserIdDefault;
        }

        public static bool IsDefaultServiceUrl()
        {
            return Current.MobileAppUrl == DefaultMobileAppUrl;
        }

        public enum AuthOption
        {
            GuestAccess, Facebook, ActiveDirectory
        }

        public string DefaultUserId
        {
            get { return AppSettings.GetValueOrDefault<string>(DefaultUserIdKey, UserIdDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DefaultUserIdKey, value); }
        }

        public string DefaultAlbumId
        {
            get { return AppSettings.GetValueOrDefault<string>(DefaultAlbumIdKey, AlbumIdDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DefaultAlbumIdKey, value); }
        }

        public string MobileAppUrl
        {
            get { return AppSettings.GetValueOrDefault<string>(MobileAppUrlKey, DefaultMobileAppUrl); }

            set { AppSettings.AddOrUpdateValue<string>(MobileAppUrlKey, value); }
        }

        public AuthOption AuthenticationType
        {
            get { return AppSettings.GetValueOrDefault<AuthOption>(AuthenticationTypeKey, DefaultAuthType); }

            set
            {
                if (AppSettings.AddOrUpdateValue<AuthOption>(AuthenticationTypeKey, value)) {
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentUserId
        {
            get { return AppSettings.GetValueOrDefault<string>(CurrentUserIdKey, DefaultUserId); }
            set { AppSettings.AddOrUpdateValue<string>(CurrentUserIdKey, value); }
        }

        private const string DefaultUserIdKey = nameof(DefaultUserIdKey);
        public const string UserIdDefault = "";

        private const string DefaultAlbumIdKey = nameof(DefaultAlbumIdKey);
        public const string AlbumIdDefault = "";

        private const string MobileAppUrlKey = nameof(MobileAppUrlKey);
        public const string DefaultMobileAppUrl = "https://contosomoments.azurewebsites.net/";

        private const string AuthenticationTypeKey = nameof(AuthenticationTypeKey);
        public const AuthOption DefaultAuthType = AuthOption.GuestAccess;

        private const string CurrentUserIdKey = nameof(CurrentUserIdKey);
        public const string DefaultCurrentUserId = "";

        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
