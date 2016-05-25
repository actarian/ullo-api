using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Ullo.Api.Views;

namespace Ullo.Models
{
    public class User
    {
        public string UserName;
        public string FacebookId;
        public string Route;
    }
    public class UserDetail : User
    {
        public string Email;
        public string FirstName;
        public string LastName;
        public bool isAuthenticated;
        public bool isAdmin;
    }
    
    public class UserSignIn
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
    public class FacebookSignIn
    {
        [Required]
        public string UserID { get; set; }

        [Required]
        public string AccessToken { get; set; }
    }

    public class UserSignUp
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string FacebookId { get; set; }
        [Required]
        public string FacebookPicture { get; set; }
        [Required]
        public string FacebookToken { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string PasswordConfirm { get; set; }
    }

}