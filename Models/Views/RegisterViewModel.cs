using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace BlogWebMVCIdentityAuth.Models.Views
{
    [NotMapped]
    public class RegisterViewModel : Author
    {
    
        [Required(ErrorMessage = "Password can not be blank.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}