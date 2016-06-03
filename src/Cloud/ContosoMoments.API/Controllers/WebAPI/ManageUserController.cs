using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Security.Claims;
using ContosoMoments.Common.Models;

namespace ContosoMoments.Api
{
    [Authorize]
    [MobileAppController]
    public class ManageUserController : ApiController
    {
        internal static async Task<string> GetUserId(HttpRequestMessage request, IPrincipal user)
        {
            ClaimsPrincipal principal = user as ClaimsPrincipal;
            string provider = principal.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider").Value;

            ProviderCredentials creds = null;
            if (string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase)) {
                creds = await user.GetAppServiceIdentityAsync<FacebookCredentials>(request);
            }
            else if (string.Equals(provider, "aad", StringComparison.OrdinalIgnoreCase)) {
                creds = await user.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(request);
            }

            return creds != null ?
                string.Format("{0}:{1}", creds.Provider, creds.Claims[ClaimTypes.NameIdentifier]) :
                null;
        }

        // return true if user is logged in with AAD
        internal static async Task<bool> IsAadLogin(HttpRequestMessage request, IPrincipal user)
        {
            ClaimsPrincipal principal = user as ClaimsPrincipal;

            var claim = principal.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider");

            if (claim == null) {
                return false;
            }

            if (string.Equals(claim.Value, "aad", StringComparison.OrdinalIgnoreCase)) {
                var creds = await user.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(request);
                return creds != null;
            }

            return false;
        }

        // GET api/ManageUser
        public async Task<string> Get()
        {
            string username = await GetUserId(Request, this.User);

            Debug.WriteLine($"Username is: {username}");

            using (var ctx = new MobileServiceContext()) {
                var user = ctx.Users.Find(username);

                if (user == null) {
                    AddUser(username, ctx);
                }

                return username;
            }
        }

        private static void AddUser(string identifier, MobileServiceContext ctx)
        {
            var u =
                ctx.Users.Add(
                    new User {
                        Id = identifier,
                    });

            ctx.SaveChanges();
        }
    }
}
