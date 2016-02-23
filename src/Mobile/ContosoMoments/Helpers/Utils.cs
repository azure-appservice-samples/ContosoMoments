using System.Threading.Tasks;
using ContosoMoments.Models;
using Xamarin.Forms;
using System;
using Newtonsoft.Json.Linq;
using ContosoMoments.Helpers;

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
                var defaults = await App.Instance.MobileService.InvokeApiAsync<JObject>("Defaults", System.Net.Http.HttpMethod.Get, null);

                if (defaults == null) {
                    retVal = false;
                }
            }
            catch (Exception)
            {
                retVal = false;
            }

            return retVal;
        }

        public static async Task PopulateDefaultsAsync()
        {
            var defaults = await App.Instance.MobileService.InvokeApiAsync<JObject>("Defaults", System.Net.Http.HttpMethod.Get, null);
 
            Settings.DefaultUserId   = defaults["DefaultUserId"].ToString();
            Settings.DefaultAlbumId  = defaults["DefaultAlbumId"].ToString();
        }
    }
}
