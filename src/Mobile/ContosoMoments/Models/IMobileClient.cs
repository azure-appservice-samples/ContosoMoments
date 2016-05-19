using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace ContosoMoments.Models
{
    public interface IMobileClient
    {
        Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider);

        /// <summary>
        /// Login using Facebook client flow
        /// </summary>
        Task<MobileServiceUser> LoginFacebookAsync();
        Task Logout();
        void ForceCloseApp();
    }
}
