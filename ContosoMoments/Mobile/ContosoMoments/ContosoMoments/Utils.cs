using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
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

            return retVal;
        }
#endif

#if __IOS__
        public static async Task<bool> CheckServerAddressIOS(string url, int port = 80, int msTimeout = 5000)
        {
            bool retVal = false;

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

            return retVal;
        }
#endif

#if __DROID__
        public static async Task<bool> CheckServerAddressDroid(string url, int port = 80, int msTimeout = 5000)
        {
            bool retVal = false;
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

            return retVal;
        }
#endif

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
    }
}
