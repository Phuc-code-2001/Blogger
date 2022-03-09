using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlogWebMVCIdentityAuth.Models.Views
{
    [NotMapped]
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username can not be blank.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password can not be blank.")]
        public string Password { get; set; } 
        public bool remember { get; set; } = true;


    }
}