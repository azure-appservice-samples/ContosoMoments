using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Server;

namespace ContosoMoments.Common.Models
{
    public class Like : EntityData
    {
        public Image Image { get; set; }

        public User User { get; set; }

        public DateTime DateTime { get; set; }
    }
}
