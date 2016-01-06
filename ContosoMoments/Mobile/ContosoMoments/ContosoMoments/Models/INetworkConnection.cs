using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoMoments.Models
{
    public interface INetworkConnection
    {
        bool IsConnected { get; }
        void CheckNetworkConnection();
    }
}
