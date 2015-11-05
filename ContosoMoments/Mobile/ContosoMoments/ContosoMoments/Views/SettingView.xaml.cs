using ContosoMoments.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if __WP__
using Windows.Networking.Sockets;
#elif __DROID__
using Java.Net;
#elif __IOS__
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class SettingView : ContentPage
    {
        public SettingView()
        {
            InitializeComponent();
        }

        public async void OnSave(object sender, EventArgs args)
        {
            if (mobileServiceUrl.Text.Length != 0)
            {
                bool ValidURL = false;

                if (mobileServiceUrl.Text.Last() != '/')
                    mobileServiceUrl.Text += "/";

#if __WP__
                ValidURL = await CheckServerAddressWP(mobileServiceUrl.Text);
#elif __IOS__
                ValidURL = await CheckServerAddressIOS(mobileServiceUrl.Text);
#elif __DROID__
                ValidURL = await CheckServerAddressDroid(mobileServiceUrl.Text);
#endif

                if (ValidURL)
                {
                    AppSettings.Current.AddOrUpdateValue<string>("MobileAppURL", mobileServiceUrl.Text);
                    Constants.ApplicationURL = AppSettings.Current.GetValueOrDefault<string>("MobileAppURL");
                    App.MobileService = new Microsoft.WindowsAzure.MobileServices.MobileServiceClient(Constants.ApplicationURL);
                    App.Current.MainPage = new NavigationPage(new ImagesList());
                }
                else
                {
                    DisplayAlert("Configuration Error", "Mobile Service URL is unreachable. Please check the URL value and try again", "OK");
                }
            }
            else
            {
                DisplayAlert("Configuration Error", "Mobile Service URL is empty. Please type in the value and try again", "OK");
            }
        }

#if __WP__
        private async Task<bool> CheckServerAddressWP(string url, int port = 80, int msTimeout = 5000)
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
        private async Task<bool> CheckServerAddressIOS(string url, int port = 80, int msTimeout = 5000)
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
        private async Task<bool> CheckServerAddressDroid(string url, int port = 80, int msTimeout = 5000)
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
    }
}
