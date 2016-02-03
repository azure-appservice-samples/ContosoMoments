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
#if __WP__
        public static async Task<bool> CheckServerAddressWP(string url, int port = 80, int msTimeout = 5000)
        {
            bool retVal = false;
            string originalUrl = url;

            if (!IsUrlValid(url))
                return retVal;

            try
            {
                using (var tcpClient = new StreamSocket())
                {
                    url = url.Replace("http://", string.Empty).Replace("https://", string.Empty);
                    if (url.Last() == '/')
                        url = url.Substring(0, url.Length - 1);

                    await tcpClient.ConnectAsync(new Windows.Networking.HostName(url), port.ToString(), SocketProtectionLevel.PlainSocket);

                    if (tcpClient.Information.RemoteHostName.ToString().ToLower() == url.ToLower())
                        retVal = true;

                    tcpClient.Dispose();
                }
            }
            catch (Exception ex)
            {
                retVal = false;
            }

            retVal &= await ExposesContosoMomentsWebAPIs(originalUrl);

            return retVal;
        }
#endif

#if __IOS__
        public static async Task<bool> CheckServerAddressIOS(string url, int port = 80, int msTimeout = 5000)
        {
            bool retVal = false;
            string originalUrl = url;

            if (!IsUrlValid(url))
                return retVal;

            url = url.Replace("http://", string.Empty).Replace("https://", string.Empty);
            if (url.Last() == '/')
                url = url.Substring(0, url.Length - 1);

            await Task.Run(() =>
            {
                try
                {
                    var clientDone = new ManualResetEvent(false);
                    var reachable = false;
                    var hostEntry = new DnsEndPoint(url, port);
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        var socketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = hostEntry };
                        socketEventArg.Completed += (s, e) =>
                        {
                            reachable = e.SocketError == SocketError.Success;

                            clientDone.Set();
                        };

                        clientDone.Reset();

                        socket.ConnectAsync(socketEventArg);

                        clientDone.WaitOne(msTimeout);

                        retVal = reachable;
                    }
                }
                catch (Exception ex)
                {
                    retVal = false;
                }
            });

            retVal &= await ExposesContosoMomentsWebAPIs(originalUrl);

            return retVal;
        }
#endif

#if __DROID__
        public static async Task<bool> CheckServerAddressDroid(string url, int port = 80, int msTimeout = 5000)
        {
            bool retVal = false;
            string originalUrl = url;

            if (!IsUrlValid(url))
                return retVal;

            await Task.Run(async () =>
            {
                try
                {
                    url = url.Replace("http://", string.Empty).Replace("https://", string.Empty);
                    if (url.Last() == '/')
                        url = url.Substring(0, url.Length - 1);

                    var sockaddr = new InetSocketAddress(url, port);
                    using (var sock = new Socket())
                    {
                        await sock.ConnectAsync(sockaddr, msTimeout);

                        if (sock.InetAddress.HostName.ToString().ToLower() == url.ToLower())
                            retVal = true;
                    }
                }
                catch (Exception ex)
                {
                    retVal = false;
                }
            });

            retVal &= await ExposesContosoMomentsWebAPIs(originalUrl);

            return retVal;
        }
#endif

        public static async Task<bool> ExposesContosoMomentsWebAPIs(string applicationURL)
        {
            bool bRes = false; //Assume Web APIs are not present

            try
            {
                var getwayService = applicationURL + "api/Getway";

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(getwayService));
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                {
                    bRes = true;
                }
            }
            catch (WebException ex)
            {
                if (ex.Message.IndexOf("Unauthorized") > 0)
                    bRes = true;
                else
                    bRes = false;
            }
            catch (Exception e)
            {
            }

            return bRes;
        }

        public static async Task<bool> IsAuthRequired(string applicationURL)
        {
            bool bRes = false; //Assume authentication is not required

            try
            {
                var getwayService = applicationURL + "api/Getway";

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(getwayService));
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                {
                    //nothing to do - authentication is not required
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse resp = (HttpWebResponse)ex.Response;
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bRes = true;
                }
            }
            catch (Exception e)
            {
            }

            return bRes;
        }

        //public static async Task<bool> DEBUG_GetFBUser(string sid)
        //{
        //    bool bRes = true;

        //    try
        //    {
        //        var fbRequestUrl = "https://graph.facebook.com/me/feed?access_token=" + sid;

        //        // Create an HttpClient request.
        //        var client = new System.Net.Http.HttpClient();

        //        // Request the current user info from Facebook.
        //        var resp = await client.GetAsync(fbRequestUrl);
        //        resp.EnsureSuccessStatusCode();

        //        // Do something here with the Facebook user information.
        //        var fbInfo = await resp.Content.ReadAsStringAsync();

        //    }
        //    catch (WebException ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //    }

        //    return bRes;
        //}

        public static bool IsOnline()
        {
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckNetworkConnection();
            return networkConnection.IsConnected;
        }

        public static async Task<bool> SiteIsOnline()
        {
            bool retVal = true;

            try
            {
                var getwayService = Constants.ApplicationURL + "api/Getway";

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(getwayService));
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                {
                    //nothing to do - authentication is not required
                }
            }
            catch
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
