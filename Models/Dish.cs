using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Ullo.Models {
    public class Dish {
        [Key]
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        public string Name { get; set; }
        [StringLength(1024)]
        public string Route { get; set; }
        public decimal Price { get; set; }
        public bool isVeganFriendly { get; set; }
        public int Yes { get; set; }
        public int No { get; set; }
        public DateTime Created { get; set; }
        [JsonIgnore]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Picture> Pictures { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public Dish()
        {
            this.Pictures = new HashSet<Picture>();
            this.Categories = new HashSet<Category>();  
        }
    }
}