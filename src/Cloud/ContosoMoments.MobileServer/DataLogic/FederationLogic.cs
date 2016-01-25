using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ContosoMoments.MobileServer.Models;
using Microsoft.Azure.Mobile.Server.Authentication;
using Newtonsoft.Json.Linq;

namespace ContosoMoments.MobileServer.DataLogic
{
    public class FederationLogic
    {
        public async Task<string> GetFacebookUserInfo(FacebookCredentials fbCredentials)
        {
           // Web.Models.ConfigModel config = new Web.Models.ConfigModel();
            string retVal = ""; //config.DefaultUserId;

            // Get the credentials for the logged-in user.
          

          
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
                    retVal = CheckOrAddEmailToDB(email);
                }
                else
                {
                    return retVal;
                }

                return retVal;
            

         
        }


        public  string GetAADUserInfo(AzureActiveDirectoryCredentials aadCredentials)
        {
            var retVal = "";
            if (null != aadCredentials && aadCredentials.Claims.Count > 0)
            {
                string email = aadCredentials.UserId;

                retVal = CheckOrAddEmailToDB(email);
            }

            return retVal;
        }

        private static string CheckOrAddEmailToDB(string email)
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