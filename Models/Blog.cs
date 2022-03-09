using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWebMVCIdentityAuth.Models
{
    [Table("Blog")]
    public class Blog {

        [Key]
        public int Id { get; set; }

        public String AuthorUserName { get; set; }

        [ForeignKey("Topic")]
        public int TopicId { get; set; }
        public Topic Topic { get; set; }

        public String HashTag { get; set; }

        public String ImageURL { get; set; }

        [Required]
        public String Title { get; set; }

        public String Content { get; set; }

        public DateTime DateTimeOfPublic { get; set; }
        
        [InverseProperty("Blog")]
        public ICollection<Like> Likes { get; set; }

    }
    
}