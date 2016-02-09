using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ContosoMoments.Models;
using Xamarin.Forms;
#if __WP__
using Windows.Networking.Sockets;
#elif __DROID__
using Java.Net;
#elif __IOS__
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif

namespace ContosoMoments
{
    public static class Utils
    {       
        public static bool IsOnline()
        {
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckNetworkConnection();
            return networkConnection.IsConnected;
        }

        public static async Task<bool> SiteIsOnline()
        {
            bool retVal = true;

            try {
                var res = await App.MobileService.InvokeApiAsync<string>("Getway", System.Net.Http.HttpMethod.Get, null);

                if (res == null && res.Length == 0) {
                    retVal = false;
                }
            }
            catch //(Exception ex)
            {
                retVal = false;
            }

            return retVal;
        }

        private static bool IsUrlValid(string url)
        {
            //string pattern = @"^(http(?:s)?\:\/\/[a-zA-Z0-9]+(?:(?:\.|\-)[a-zA-Z0-9]+)+(?:\:\d+)?(?:\/[\w\-]+)*(?:\/?|\/\w+\.[a-zA-Z]{2,4}(?:\?[\w]+\=[\w\-]+)?)?(?:\&[\w]+\=[\w\-]+)*)$";
            string pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }
    }
}
