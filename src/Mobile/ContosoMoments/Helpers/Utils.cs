using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ContosoMoments.Models;
using Xamarin.Forms;

namespace ContosoMoments
{
    public class ResizeRequest
    {
        public string Id { get; set; }
        public string BlobName { get; set; }
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
