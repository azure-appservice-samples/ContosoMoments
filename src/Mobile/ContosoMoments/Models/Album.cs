using System;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace ContosoMoments.Models
{
    public class Album
    {
        [JsonProperty(PropertyName = "Id")]
        public string AlbumId { get; set; }

        public string AlbumName { get; set; }
        public bool IsDefault { get; set; }
        public string UserId { get; set; }

        [CreatedAt]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
