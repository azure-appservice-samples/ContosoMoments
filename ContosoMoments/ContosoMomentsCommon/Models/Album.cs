using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Mobile.Service;


namespace ContosoMoments.Common.Models
{
    public class Album : EntityData
    {
        public Guid AlbumId { get; set; }

        public string AlbumName { get; set; }

        public IEnumerable<Image> Images { get; set; }

        public User User { get; set; }
    }
}
