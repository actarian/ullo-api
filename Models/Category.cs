using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ullo.Models
{
    public class Category
    {
        [Key]
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        public string Name { get; set; }
        [StringLength(1024)]
        public string Route { get; set; }
        // [JsonIgnore]
        public virtual ICollection<Dish> Dishes { get; set; }
        public Category()
        {
            this.Dishes = new HashSet<Dish>();
        }
    }
}