using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Ullo.Models
{
    public class Post
    {
        [Key]
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        public DateTime Created { get; set; }
        [JsonIgnore]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        [JsonIgnore]
        public int DishId { get; set; }
        public virtual Dish Dish { get; set; }
        [JsonIgnore]
        public int PictureId { get; set; }
        public virtual Picture Picture { get; set; }
    }
}