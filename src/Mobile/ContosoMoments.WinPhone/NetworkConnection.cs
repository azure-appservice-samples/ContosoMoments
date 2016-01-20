using ContosoMoments.Models;
using ContosoMoments.WinPhone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkConnection))]
namespace ContosoMoments.WinPhone
{
    public class NetworkConnection : INetworkConnection
    {
        public bool IsConnected { get; set; }

        public void CheckNetworkConnection()
        {
            IsConnected = false;

            var connections = NetworkInformation.GetConnectionProfiles().ToList();
            connections.Add(NetworkInformation.GetInternetConnectionProfile());

            foreach (var connection in connections)
            {
                if (connection == null)
                    continue;

                if (connection.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                {
                    IsConnected = true;
                    break;
                }
            }
        }
    }
}
