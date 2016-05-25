using Microsoft.Azure.Mobile.Server;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ContosoMoments.Common.Models
{
    public class Album : EntityData
    {
        public string AlbumName { get; set; }

        public bool IsDefault { get; set; }

        [JsonIgnore]
        public virtual ICollection<Image> Images { get; set; }

        public virtual User User { get; set; }

        [Column("User_Id")]
        public string UserId { get; set; }
    }
}
