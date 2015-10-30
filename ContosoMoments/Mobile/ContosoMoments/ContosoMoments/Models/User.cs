using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoMoments.Models
{
    public class User
    {
        Guid userId;
        string userName;
        bool isEnabled;

        [JsonProperty(PropertyName = "Id")]
        public Guid UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        [JsonProperty(PropertyName = "UserName")]
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        [JsonProperty(PropertyName = "IsEnabled")]
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        [Version]
        public string Version { get; set; }
    }
}
