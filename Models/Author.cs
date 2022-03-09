using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace BlogWebMVCIdentityAuth.Models {
    
    [Table("Author")]
    public class Author : IdentityUser<Guid>
    {
        [Required]
        [MinLength(8)]
        public String Fullname { get; set; }

        public String ImageUrl { get; set; } = "/media/avatardefault.png";

        [Required]
        public int Gender { get; set; } = 1;

        [NotMapped]
        public String GenderText {
            get {
                return this.Gender == 1 ? "Male" : this.Gender == 0 ? "Female" : "Unknown";
            }
        }

        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; } = DateTime.Today;

        public String Story { get; set; }

        public DateTime DateJoin { get; set; } = DateTime.Today;
        
    }

}