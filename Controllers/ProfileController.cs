using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlogWebMVCIdentityAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using BlogWebMVCIdentityAuth.Models.Views;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using BlogWebMVCIdentityAuth.Data;
using Microsoft.EntityFrameworkCore;
using BlogWebMVCIdentityAuth.DataStructures;
using System.Net.Http;

namespace BlogWebMVCIdentityAuth.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        readonly UserManager<Author> _userManager;
        readonly SignInManager<Author> _signInManager;
        readonly ILogger<IdentityController> _logger;
        readonly ApplicationDbContext _context;
        
        public ProfileController(ILogger<IdentityController> logger, UserManager<Author> userManager, 
        SignInManager<Author> signInManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("{UserName}")]
        public async Task<IActionResult> Index(String UserName, int page = 1)
        {
            Author User = await _userManager.FindByNameAsync(UserName);
            if(User is null) return NotFound();
            
            var blogs = _context.Blogs.Include(item => item.Topic).Include(item => item.Likes)
                                .Where(item => item.AuthorUserName == User.UserName);

            blogs = blogs.OrderByDescending(item => item.DateTimeOfPublic);

            int pageSize = UserName.Equals(HttpContext.User.Identity.Name) ? 3 : 8;
            var paginatedBlogs = await PaginatedList<Blog>.CreateAsync(blogs.AsNoTracking(), page, pageSize);

            if(paginatedBlogs.TotalPages > 0 && paginatedBlogs.PageIndex > paginatedBlogs.TotalPages) return NotFound();

            ViewData["Blogs"] = paginatedBlogs;

            int totalLikes = 0;
            foreach(Blog item in blogs) totalLikes += item.Likes.Count;

            ViewData["LikeCounterOfAuthor"] = totalLikes;
            ViewData["BlogCounterOfAuthor"] = blogs.Count();

            ViewData["MTopic"] = await blogs.GroupBy(item => item.Topic.Name).OrderByDescending(group => group.Count())
            .Select(group => group.Key).Where(name => name != "Other").FirstOrDefaultAsync();

            ViewData["FollowCounter"] = await _context.FollowObjects.CountAsync(item => item.TargetUserName == UserName);

            ViewData["isFollow"] = _context.FollowObjects.Any(item => item.FollowerUserName.Equals(HttpContext.User.Identity.Name) && item.TargetUserName == UserName);

            if(UserName.Equals(HttpContext.User.Identity.Name)) {
                IEnumerable<Following> Followings = _context.FollowObjects.Where(item => item.FollowerUserName == UserName).ToList();
                ViewData["Followings"] = Followings;
            }

            return View(User);
        }

        public async Task<IActionResult> UpdateImageAsync(IFormFile file) {

            if(file == null) return BadRequest();

            string LoginUserName = HttpContext.User.Identity.Name;

            String ext = Path.GetExtension(file.FileName);
            List<String> image_extensions = new List<string>() {".jpg", ".png", ".jpeg", ".gif"};
            if(!image_extensions.Contains(ext)) return BadRequest();

            string Name = $"user_{LoginUserName}" + Path.GetExtension(file.FileName);
            
            //Get url To Save
            string RelativeSaveDirectory = $"/media/users/{LoginUserName}/";

            string AbsoluteSaveFullPath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/wwwroot" + RelativeSaveDirectory + Name;

            if(Directory.Exists(Path.GetDirectoryName(AbsoluteSaveFullPath)) == false) {
                Directory.CreateDirectory(Path.GetDirectoryName(AbsoluteSaveFullPath));
            }

            using (FileStream stream = new FileStream(AbsoluteSaveFullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            Author LoginUser = await _userManager.FindByNameAsync(LoginUserName);
            LoginUser.ImageUrl = $"{RelativeSaveDirectory}{Name}?last_update={-DateTime.Now.ToBinary()}";
            IdentityResult result = await _userManager.UpdateAsync(LoginUser);
            if(result.Succeeded) {
                AlertMessage message = new AlertMessage() {Type="success", Label="Success", Content="Your avatar updated successfully."};
                TempData["RedirectMessage"] = JsonConvert.SerializeObject(message);
                return RedirectToAction("Index", "Profile", new {UserName=LoginUserName});
            }
            
            return StatusCode(500);
        }

        public async Task<IActionResult> UpdateInfoAsync(Author PostedAuthorData) {

            Author LoginUser   = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            if(PostedAuthorData.PhoneNumber == null || Regex.IsMatch(PostedAuthorData.PhoneNumber, "^[0-9]{10,11}$")) {

                LoginUser.PhoneNumber = PostedAuthorData.PhoneNumber;
                LoginUser.Gender      = PostedAuthorData.Gender;
                LoginUser.Birthday    = PostedAuthorData.Birthday;

                IdentityResult result = await _userManager.UpdateAsync(LoginUser);
                if(result.Succeeded) {
                    AlertMessage message = new AlertMessage() {Type="success", Label="Success", Content="Your info updated successfully."};
                    TempData["RedirectMessage"] = JsonConvert.SerializeObject(message);
                } else {
                    foreach (IdentityError error in result.Errors)
                    {
                        AlertMessage message = new AlertMessage() {Type="danger", Label="Error", Content=error.Description};
                        TempData["RedirectMessage"] = JsonConvert.SerializeObject(message);
                        break;
                    }
                }
            } else {
                AlertMessage message = new AlertMessage() {Type="danger", Label="Error", Content="Your PhoneNumber Invalid. Try again."};
                TempData["RedirectMessage"] = JsonConvert.SerializeObject(message);
            }


            return RedirectToAction("Index", "Profile", new {UserName=LoginUser.UserName});
        }

        public async Task<IActionResult> UpdateStoryAsync(String text) {
            
            Author LoginUser = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            
            if(text != null && text.Length > 100) {
                AlertMessage message = new AlertMessage() {Type="danger", Label="Too Long", Content="Your story's size should be less than 100 characters."};
                TempData["RedirectMessage"] = JsonConvert.SerializeObject(message);
            }
            else {

                LoginUser.Story  = text;
                IdentityResult result = await _userManager.UpdateAsync(LoginUser);
                if(result.Succeeded) {
                    AlertMessage message = new AlertMessage() {Type="success", Label="Success", Content="Your story updated successfully."};
                    TempData["RedirectMessage"] = JsonConvert.SerializeObject(message);
                }
                else {
                    AlertMessage message = new AlertMessage() {Type="danger", Label="Server Error", Content="Had an error when update your infomation."};
                    TempData["RedirectMessage"] = JsonConvert.SerializeObject(message);
                }
            }

            return RedirectToAction("Index", "Profile", new {UserName=LoginUser.UserName});

        }

    }
}