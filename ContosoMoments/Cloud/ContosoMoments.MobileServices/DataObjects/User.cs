using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoMomentsMobileWeb.DataObjects
{
    public class User : EntityData
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public bool IsEnabled { get; set; }
    }
}