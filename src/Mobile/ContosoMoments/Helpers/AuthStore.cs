using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Auth;
using Xamarin.Forms;

namespace ContosoMoments.Helpers
{
    public class AuthStore
    {
        private static string TokenKeyName = "token";

        public static void CacheAuthToken(MobileServiceUser user)
        {
            var account = new Account(user.UserId);
            account.Properties.Add(TokenKeyName, user.MobileServiceAuthenticationToken);
            GetAccountStore().Save(account, App.AppName);

            Debug.WriteLine($"Cached auth token: {user.MobileServiceAuthenticationToken}");
        }

        public static MobileServiceUser GetUserFromCache()
        {
            var account = GetAccountStore().FindAccountsForService(App.AppName).FirstOrDefault();

            if (account == null) {
                return null;
            }

            var token = account.Properties[TokenKeyName];
            Debug.WriteLine($"Retrieved token from account store: {token}");

            return new MobileServiceUser(account.Username) {
                MobileServiceAuthenticationToken = token
            };
        }

        public static void DeleteTokenCache()
        {
            var accountStore = GetAccountStore();
            var account = accountStore.FindAccountsForService(App.AppName).FirstOrDefault();
            if (account != null) {
                accountStore.Delete(account, App.AppName);
            }
        }

        private static AccountStore GetAccountStore()
        {
            return DependencyService.Get<IPlatform>().GetAccountStore();
        }
    }
}
