using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogWebMVCIdentityAuth.Data;
using BlogWebMVCIdentityAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogWebMVCIdentityAuth.Controllers
{
    [Authorize]
    public class LikeController : Controller {

        readonly ApplicationDbContext _context;

        public LikeController(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index(int blog_id) {

            Blog item = await _context.Blogs.FindAsync(blog_id);
            if(item == null) return NotFound();

            String UserName = User.Identity.Name;
            
            if(_context.LikeObjects.Any(like => like.BlogId == blog_id && like.UserName == UserName)) {
                Like likeObject = _context.LikeObjects.FirstOrDefault(like => like.BlogId == blog_id && like.UserName == UserName);
                _context.LikeObjects.Remove(likeObject);
                await _context.SaveChangesAsync();
                return Ok(new {
                    operationType="DisLike",
                    CurrentLikeCounter=_context.LikeObjects.Count(like => like.BlogId == blog_id)
                });
            };

            await _context.LikeObjects.AddAsync(new Like() {
                UserName = UserName,
                BlogId = blog_id
            });
            await _context.SaveChangesAsync();
            return Ok(new {
                operationType="Like",
                CurrentLikeCounter=_context.LikeObjects.Count(like => like.BlogId == blog_id)
            });
        }

        

    }
}