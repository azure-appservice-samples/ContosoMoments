using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoMoments.Models
{
    public class Album
    {
        string albumId;
        string albumName;
        bool isDefault;
        //bool isDeleted;
        string userId;

        [JsonProperty(PropertyName = "Id")]
        public string AlbumId
        {
            get { return albumId; }
            set { albumId = value; }
        }

        [JsonProperty(PropertyName = "AlbumName")]
        public string AlbumName
        {
            get { return albumName; }
            set { albumName = value; }
        }

        [JsonProperty(PropertyName = "IsDefault")]
        public bool IsDefault
        {
            get { return isDefault; }
            set { isDefault = value; }
        }

        [JsonProperty(PropertyName = "UserId")]
        public string UserId
        {
            get { return userId; }
            set { userId = value; }
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
