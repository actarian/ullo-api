using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ullo.Models
{
    public class Vote
    {
        [Key, Column(Order = 0)]
        public int DishId { get; set; }
        [Key, Column(Order = 1)]        
        public string UserId { get; set; }
        public DateTime Created { get; set; }
        public bool Like { get; set; }     
    }
}