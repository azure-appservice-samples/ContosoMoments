using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoMomentsMobileWeb.DataObjects
{
    public class Album : EntityData
    {
        public Guid AlbumId { get; set; }

        public string AlbumName { get; set; }
    }
}