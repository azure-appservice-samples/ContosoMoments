using Newtonsoft.Json;
using System;

namespace ContosoMoments.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "Id")]
        public Guid UserId { get; set; }

        [JsonProperty(PropertyName = "Email")]
        public string UserName { get; set; }

        public bool IsEnabled { get; set; }
    }
}
