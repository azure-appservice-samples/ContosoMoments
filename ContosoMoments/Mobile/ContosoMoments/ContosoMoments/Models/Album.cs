using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoMoments.Models
{
    public class Album
    {
        Guid albumId;
        string albumName;

        [JsonProperty(PropertyName = "Id")]
        public Guid AlbumId
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

        [Version]
        public string Version { get; set; }
    }
}
