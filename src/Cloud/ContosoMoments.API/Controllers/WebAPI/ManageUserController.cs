using ContosoMoments.MobileServer.Models;
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

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class ManageUserController : ApiController
    {
        protected readonly string _defaultUserId;
        protected const string FACEBOOK_GRAPH_URL = "https://graph.facebook.com/v2.5/me?fields=email%2Cfirst_name%2Clast_name&access_token=";

        public ManageUserController()
        {
            Web.Models.ConfigModel config = new Web.Models.ConfigModel();
            _defaultUserId = config.DefaultUserId;
        }

        // GET api/ManageUser
        public async Task<string> Get()
        {
            string retVal = default(string);

            // Get the credentials for the logged-in user.
            var fbCredentials = await User.GetAppServiceIdentityAsync<FacebookCredentials>(Request);
            if (null != fbCredentials && fbCredentials.Claims.Count > 0)
            {
                retVal = CheckAddEmailToDB(await GetEmailFromFacebookGraph(fbCredentials.AccessToken));
            }

            var aadCredentials = await User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(Request);
            if (null != aadCredentials && aadCredentials.Claims.Count > 0)
            {
                retVal = CheckAddEmailToDB(aadCredentials.UserId);
            }

            return UserOrDefault(retVal);
        }

        // POST: api/Like
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

        private string UserOrDefault(string retVal)
        {
            if (string.IsNullOrWhiteSpace(retVal))
            {
                retVal = _defaultUserId;
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
                return AddUser(email, ctx);
            }
        }

        private static string AddUser(string email, MobileServiceContext ctx)
        {
            var u = ctx.Users.Add(
                new Common.Models.User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email,
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
