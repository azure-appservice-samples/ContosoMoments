using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ContosoMoments.Models
{
    public interface IMobileClient
    {
        Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider);
        void Logout();
    }
}
