using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;

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

        [JsonProperty(PropertyName = "Email")]
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

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        [Deleted]
        public bool Deleted { get; set; }
    }
}
