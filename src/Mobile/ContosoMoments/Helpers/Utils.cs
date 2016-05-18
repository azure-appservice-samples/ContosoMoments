using ContosoMoments.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices.Eventing;

namespace ContosoMoments
{
    public class ResizeRequest
    {
        public string Id { get; set; }
        public string BlobName { get; set; }
    }

    public class ImageDownloadEvent : MobileServiceEvent
    {
        public string Id { get; set; }

        public ImageDownloadEvent(string id) : base(id)
        {
            this.Id = id;
        }        
    }

    public class ActivityIndicatorScope : IDisposable
    {
        private ActivityIndicator indicator;

        public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
        {
            this.indicator = indicator;

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

        private static async Task<JObject> GetDefaultsAsync()
        {
            return await App.Instance.MobileService.InvokeApiAsync<JObject>("Defaults", System.Net.Http.HttpMethod.Get, null);
        }

        public static async Task<bool> SiteIsOnline()
        {
            bool retVal = true;

            try {
                var defaults = await GetDefaultsAsync();

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
            var defaults = await GetDefaultsAsync();
 
            Settings.Current.DefaultUserId   = defaults["DefaultUserId"].ToString();
            Settings.Current.DefaultAlbumId  = defaults["DefaultAlbumId"].ToString();
        }
    }
}
