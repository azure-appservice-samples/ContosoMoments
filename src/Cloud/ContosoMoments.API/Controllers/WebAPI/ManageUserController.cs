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

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class ManageUserController : ApiController
    {
        // GET api/ManageUser
        public async Task<string> Get()
        {
            Web.Models.ConfigModel config = new Web.Models.ConfigModel();
            string retVal = config.DefaultUserId;


            // Get the credentials for the logged-in user.
            var fbCredentials = await User.GetAppServiceIdentityAsync<FacebookCredentials>(Request);

            if (null != fbCredentials && fbCredentials.Claims.Count > 0)
            {
                // Create a query string with the Facebook access token.
                var fbRequestUrl = "https://graph.facebook.com/v2.5/me?fields=email%2Cfirst_name%2Clast_name&access_token="
                    + fbCredentials.AccessToken;

                // Create an HttpClient request.
                var client = new System.Net.Http.HttpClient();

                // Request the current user info from Facebook.
                var resp = await client.GetAsync(fbRequestUrl);
                resp.EnsureSuccessStatusCode();

                // Do something here with the Facebook user information.
                var fbInfo = await resp.Content.ReadAsStringAsync();

                JObject fbObject = JObject.Parse(fbInfo);
                var emailToken = fbObject.GetValue("email");

                if (null != emailToken)
                {
                    string email = emailToken.ToString();
                    retVal = CheckAddEmailToDB(email);
                }

                return retVal;
            }

            var aadCredentials = await User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(Request);
            if (null != aadCredentials && aadCredentials.Claims.Count > 0)
            {
                string email = aadCredentials.UserId;
                retVal = CheckAddEmailToDB(email);
            }

            return retVal;
        }
        // POST: api/Like

        public async Task<string> Get(string data, string provider)
        {
            Web.Models.ConfigModel config = new Web.Models.ConfigModel();
            string retVal = config.DefaultUserId;

            if (provider == "facebook")
            {
                // Create a query string with the Facebook access token.
                var fbRequestUrl = "https://graph.facebook.com/v2.5/me?fields=email%2Cfirst_name%2Clast_name&access_token="
                                   + data;

                // Create an HttpClient request.
                var client = new System.Net.Http.HttpClient();

                // Request the current user info from Facebook.
                var resp = await client.GetAsync(fbRequestUrl);
                resp.EnsureSuccessStatusCode();

                // Do something here with the Facebook user information.
                var fbInfo = await resp.Content.ReadAsStringAsync();

                JObject fbObject = JObject.Parse(fbInfo);
                var emailToken = fbObject.GetValue("email");

                if (null != emailToken)
                {
                    string email = emailToken.ToString();
                    retVal = CheckAddEmailToDB(email);
                }

                return retVal;
            }

            //  var aadCredentials = await this.User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(this.Request);
            if (provider == "aad")
            {
                string email = data;
                retVal = CheckAddEmailToDB(email);
            }
            return retVal;
        }

        private static string CheckAddEmailToDB(string email)
        {
            var identifier = GenerateHashFromEmail(email);

            using (var ctx = new MobileServiceContext())
            {
                var user = ctx.Users.FirstOrDefault(x => x.Email == identifier);

                if (default(Common.Models.User) != user)
                    return user.Id; // User Found, Exit

                // New User, Create
                return AddUser(identifier, ctx);
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
