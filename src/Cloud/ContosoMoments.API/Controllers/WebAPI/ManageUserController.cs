using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Diagnostics;
using ContosoMoments.API;
using ContosoMoments.API.Helpers;
using System.Net.Http;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    [Authorize]
    public class ManageUserController : ApiController
    {
        protected const string FACEBOOK_GRAPH_URL = "https://graph.facebook.com/v2.5/me?fields=email%2Cfirst_name%2Clast_name&access_token=";

        internal static async Task<string> GetUserId(HttpRequestMessage request, IPrincipal user)
        {
            string result = "";

            // Get the credentials for the logged-in user.
            var fbCredentials = await user.GetAppServiceIdentityAsync<FacebookCredentials>(request);
            var aadCredentials = await user.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(request);

            if (fbCredentials?.Claims?.Count > 0) {
                result = CheckAddEmailToDB(await GetEmailFromFacebookGraph(fbCredentials.AccessToken));
            }
            else if (aadCredentials?.Claims?.Count > 0) {
                result = CheckAddEmailToDB(aadCredentials.UserId);
            }

            return UserOrDefault(result);
        }

        // GET api/ManageUser
        public async Task<string> Get()
        {
            return await GetUserId(Request, User);
        }

        public async Task<string> Get(string data, string provider)
        {
            string retVal = default(string);

            if (provider.Equals("facebook"))
            {
                retVal = CheckAddEmailToDB(await GetEmailFromFacebookGraph(data));
            }

            if (provider.Equals("aad"))
            {
                retVal = CheckAddEmailToDB(data);
            }

            return UserOrDefault(retVal);
        }

        private static string UserOrDefault(string retVal)
        {
            if (string.IsNullOrWhiteSpace(retVal))
            {
                retVal = new ConfigModel().DefaultUserId;
            }

            return retVal;
        }

        private static async Task<string> GetEmailFromFacebookGraph(string credentials)
        {
            string fbInfo = default(string);
            // Create a query string with the Facebook access token.
            var fbRequestUrl = FACEBOOK_GRAPH_URL + credentials;

            using (var client = new System.Net.Http.HttpClient())
            {
                // Request the current user info from Facebook.
                var resp = await client.GetAsync(fbRequestUrl);
                resp.EnsureSuccessStatusCode();

                // Do something here with the Facebook user information.
                fbInfo = await resp.Content.ReadAsStringAsync();
                Trace.WriteLine("fbInfo: " + fbInfo);
            }

            JObject fbObject = JObject.Parse(fbInfo);
            var emailToken = fbObject.GetValue("email");
            Trace.WriteLine("email: " + emailToken);

            return emailToken.ToString();
        }

        private static string CheckAddEmailToDB(string email)
        {
            var identifier = GenerateHashFromEmail(email);

            using (var ctx = new MobileServiceContext())
            {
                var user = ctx.Users.FirstOrDefault(x => x.Email == identifier);

                // user was found, return it
                if (user != default(Common.Models.User)) {
                    return user.Id;
                }

                // create new user
                return AddUser(identifier, ctx);
            }
        }

        private static string AddUser(string emailHash, MobileServiceContext ctx)
        {
            var u = ctx.Users.Add(
                new Common.Models.User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = emailHash,
                    IsEnabled = true
                });

            try
            {
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return u.Id;
        }

        private static string GenerateHashFromEmail(string email)
        {
            StringBuilder hashString = new StringBuilder();

            using (var generator = System.Security.Cryptography.SHA256.Create())
            {
                var emailBytes = Encoding.UTF8.GetBytes(email);
                var hash = generator.ComputeHash(emailBytes);

                foreach (var b in hash)
                {
                    hashString.AppendFormat("{0:x2}", b);
                }
            }

            return hashString.ToString();
        }
    }
}
