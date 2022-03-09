using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogWebMVCIdentityAuth.Models
{
    [Table("Like")]
    public class Like {

        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }

        [ForeignKey("Blog")]
        public int BlogId { get; set; }
        public Blog Blog { get; set; }

    }
}