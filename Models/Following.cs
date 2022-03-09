

namespace BlogWebMVCIdentityAuth.Models
{
    public class Following {

        public int Id { get; set; }

        public string FollowerUserName { get; set; }

        public string TargetUserName { get; set; }

    }
}