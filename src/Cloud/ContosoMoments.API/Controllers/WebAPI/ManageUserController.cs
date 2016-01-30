using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Threading.Tasks;
using System.Security.Principal;
using Microsoft.Azure.Mobile.Server.Authentication;
using System.Linq;
using Newtonsoft.Json.Linq;
using ContosoMoments.MobileServer.Models;
using System;
using ContosoMoments.Common.Queue;
using ContosoMoments.Common;

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
            var fbCredentials = await this.User.GetAppServiceIdentityAsync<FacebookCredentials>(this.Request);

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
                else
                {
                    return retVal;
                }

                return retVal;
            }

            var aadCredentials = await this.User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(this.Request);
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
            string retVal;
            var ctx = new MobileServiceContext();
            var user = ctx.Users.Where(x => x.Email == email);

            if (user.Count() == 0)
            {
                var u = ctx.Users.Add(new Common.Models.User() { Id = Guid.NewGuid().ToString(), Email = email, IsEnabled = true });
                try
                {
                    ctx.SaveChanges();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    //
                }

                retVal = u.Id;
            }
            else
            {
                var u = user.First();
                retVal = u.Id;
            }

            return retVal;
        }
    }
}
