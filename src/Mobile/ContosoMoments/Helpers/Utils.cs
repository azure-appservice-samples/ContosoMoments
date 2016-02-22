using ContosoMoments.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments
{
    public class ResizeRequest
    {
        public string Id { get; set; }
        public string BlobName { get; set; }
    }

    public class ActivityIndicatorScope : IDisposable
    {
        private bool showIndicator;
        private ActivityIndicator indicator;

        public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
        {
            this.indicator = indicator;
            this.showIndicator = showIndicator;

            SetIndicatorActivity(showIndicator);
        }

        private void SetIndicatorActivity(bool isActive)
        {
            this.indicator.IsVisible = isActive;
            this.indicator.IsRunning = isActive;
        }

        public void Dispose()
        {
            SetIndicatorActivity(false);
        }
    }

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
    }
}
