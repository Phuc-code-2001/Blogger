using System.Linq;
using System.Threading.Tasks;
using BlogWebMVCIdentityAuth.Data;
using BlogWebMVCIdentityAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogWebMVCIdentityAuth.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {

        readonly ApplicationDbContext _context;

        public FollowController(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync(string UserName)
        {
            string LoginUserName = User.Identity.Name;

            if(LoginUserName == UserName) return BadRequest(new {message="You can not follow yourself."});

            Following checker = _context.FollowObjects
            .FirstOrDefault(item => item.FollowerUserName == LoginUserName && item.TargetUserName == UserName);

            if(checker != null) {
                // UnFollow
                _context.FollowObjects.Remove(checker);
                await _context.SaveChangesAsync();
                return Ok(_context.FollowObjects.Count(item => item.TargetUserName == UserName));
            }
            else {
                // Follow
                _context.FollowObjects.Add(new Following() {
                    FollowerUserName = LoginUserName,
                    TargetUserName = UserName
                });
                await _context.SaveChangesAsync();
                return Ok(_context.FollowObjects.Count(item => item.TargetUserName == UserName));
            }

        }
    }
}