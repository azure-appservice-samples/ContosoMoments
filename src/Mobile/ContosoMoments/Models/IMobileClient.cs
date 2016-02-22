using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace ContosoMoments.Models
{
    public interface IMobileClient
    {
        Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider);
        void Logout();
        void ForceCloseApp();
    }
}
