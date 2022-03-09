using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlogWebMVCIdentityAuth.Models;
using BlogWebMVCIdentityAuth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using BlogWebMVCIdentityAuth.DataStructures;

namespace BlogWebMVCIdentityAuth.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        readonly UserManager<Author> _userManager;
        readonly SignInManager<Author> _signInManager;
        readonly ApplicationDbContext _context;
        readonly ILogger<IdentityController> _logger;

        public BlogController(ILogger<IdentityController> logger, UserManager<Author> userManager, 
        SignInManager<Author> signInManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            Topic.SeedData(context);
        }

        [AllowAnonymous]
        [HttpGet("Blogs")]
        public async Task<IActionResult> Index(int topic_id, int page = 1)
        {
            if(topic_id != 0) return await ListByTopicAsync(topic_id, page);

            ViewData["TopicList"]  = _context.Topics.ToList();

            if(User.Identity.IsAuthenticated) {
                List<String> Followers = _context.FollowObjects
                .Where(item => item.FollowerUserName == User.Identity.Name).Select(item => item.TargetUserName).ToList();
                Followers.Insert(0, User.Identity.Name);

                var BlogRecommended = _context.Blogs.Include(item => item.Topic).Include(item => item.Likes)
                                .OrderByDescending(item => Followers.Contains(item.AuthorUserName))
                                .ThenByDescending(item => item.DateTimeOfPublic)
                                .ThenByDescending(item => item.Likes.Count());
                                
                int pageSize = 5;
                var paginatedBlogRecommended = await PaginatedList<Blog>.CreateAsync(BlogRecommended.AsNoTracking(), page, pageSize);

                if(paginatedBlogRecommended.TotalPages > 0 && paginatedBlogRecommended.PageIndex > paginatedBlogRecommended.TotalPages) return NotFound();

                ViewData["Recommended"] = paginatedBlogRecommended;
                
            } else {
                var BlogRecommended = _context.Blogs.Include(item => item.Topic).Include(item => item.Likes)
                .OrderByDescending(item => item.Likes.Count).ThenByDescending(item => item.DateTimeOfPublic);

                int pageSize = 5;
                var paginatedBlogRecommended = await PaginatedList<Blog>.CreateAsync(BlogRecommended.AsNoTracking(), page, pageSize);

                if(paginatedBlogRecommended.TotalPages > 0 && paginatedBlogRecommended.PageIndex > paginatedBlogRecommended.TotalPages) return NotFound();

                ViewData["Recommended"] = paginatedBlogRecommended;
            }

            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> DetailAsync(int blog_id) {

            Blog model = await _context.Blogs.FindAsync(blog_id);

            if(model == null) return NotFound();
            model.Likes = _context.LikeObjects.Where(item => item.BlogId == blog_id).ToList();

            if(User.Identity.IsAuthenticated) ViewData["isLike"] = model.Likes.Any(item => item.UserName == User.Identity.Name);

            return View(model);

        }


        public async Task<IActionResult> EditAsync(int blog_id) {
            Blog model = await _context.Blogs.FindAsync(blog_id);
            if(model == null) return NotFound();

            if(model.AuthorUserName != User.Identity.Name) return Unauthorized();

            ViewData["TopicList"] = new SelectList(_context.Topics, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(Blog Object, IFormFile ImageFileRaw) {

            List<String> Errors = new List<string>();

            if(ModelState.IsValid) {

                if(ImageFileRaw != null) {

                    String ext = Path.GetExtension(ImageFileRaw.FileName);
                    List<String> image_extensions = new List<string>() {".jpg", ".png", ".jpeg", ".gif"};
                    if(image_extensions.Contains(ext)) Object.ImageURL = SaveImage(ImageFileRaw);

                }

                Object.DateTimeOfPublic = DateTime.Now;
                _context.Blogs.Update(Object);
                await _context.SaveChangesAsync();

                return RedirectToAction("Detail", new {blog_id=Object.Id});
                
            }

            Errors.AddRange(ModelState.Values.SelectMany(item => item.Errors).Select(error => error.ErrorMessage).ToList());
            ViewData["Errors"] = Errors;
            ViewData["TopicList"] = new SelectList(_context.Topics, "Id", "Name");
            return View("Edit", Object);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ListByTopicAsync(int topic_id, int page = 1) {

            
            Topic model = await _context.Topics.Include(item => item.Blogs).FirstAsync(item => item.Id == topic_id);
            if(model == null) return NotFound();

            return View("ListByTopic", model);
        }


        [HttpGet("/Blogs/Create")]
        public async Task<IActionResult> CreateAsync(int topic_id)
        {
            Blog instance = new Blog();
            Topic _Topic = await _context.Topics.FindAsync(topic_id);

            if(_Topic != null) {
                instance.TopicId = topic_id;
                instance.Topic   = _Topic;
            }

            ViewData["TopicList"] = new SelectList(_context.Topics, "Id", "Name");
            
            return View(instance);
        }


        [HttpPost("/Blogs/Create")]
        public async Task<IActionResult> CreateAsync(Blog Object, IFormFile ImageFileRaw) {

            List<String> Errors = new List<string>();
            if(ImageFileRaw == null) Errors.Add("Please choose a image to presentation!");

            else if(ModelState.IsValid) {

                String ext = Path.GetExtension(ImageFileRaw.FileName);
                List<String> image_extensions = new List<string>() {".jpg", ".png", ".jpeg", ".gif"};
                if(!image_extensions.Contains(ext)) Errors.Add("File format invalid!");

                else {

                    Object.ImageURL = SaveImage(ImageFileRaw);
                    Object.DateTimeOfPublic = DateTime.Now;

                    await _context.Blogs.AddAsync(Object);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Detail", new {blog_id=Object.Id});
                }
                
            }

            Errors.AddRange(ModelState.Values.SelectMany(item => item.Errors).Select(error => error.ErrorMessage).ToList());
            ViewData["Errors"] = Errors;
            ViewData["TopicList"] = new SelectList(_context.Topics, "Id", "Name");
            return View("Create", Object);
        }

        public String SaveImage(IFormFile ImageFile) {

            string Name = $"img_{Guid.NewGuid()}" + Path.GetExtension(ImageFile.FileName);
            
            //Get url To Save
            string RelativeSaveDirectory = $"/media/BlogPhotos/";
            string AbsoluteSaveFullPath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/wwwroot" + RelativeSaveDirectory + Name;

            if(Directory.Exists(Path.GetDirectoryName(AbsoluteSaveFullPath)) == false) {
                Directory.CreateDirectory(Path.GetDirectoryName(AbsoluteSaveFullPath));
            }
            using (FileStream stream = new FileStream(AbsoluteSaveFullPath, FileMode.Create))
            {
                ImageFile.CopyTo(stream);
            }

            return $"{RelativeSaveDirectory}{Name}?last_update={-DateTime.Now.ToBinary()}";
        }

        [AllowAnonymous]
        [HttpGet("Blog/Search")]
        public async Task<IActionResult> SearchAsync(string keyword, List<string> search_by, int page = 1)
        {
            IQueryable<Blog> query = _context.Blogs.Include(item => item.Topic);

            List<List<Blog>> temp_result = new List<List<Blog>>();
            if (search_by.Contains("author")) temp_result.Add(await query.Where(item => item.AuthorUserName.Contains(keyword)).ToListAsync());
            if (search_by.Contains("title")) temp_result.Add(await query.Where(item => item.Title.Contains(keyword)).ToListAsync());
            if (search_by.Contains("hashtag")) temp_result.Add(await query.Where(item => item.HashTag.Contains(keyword)).ToListAsync());
            if (search_by.Contains("content")) temp_result.Add(await query.Where(item => item.Content.Contains(keyword)).ToListAsync());

            IEnumerable<Blog> result = new List<Blog>();
            foreach(List<Blog> set in temp_result) {
                result = result.Union(set);
            }

            int pageSize = 5;
            var pResult = PaginatedList<Blog>.Create(result.AsQueryable(), page, pageSize);

            if(pResult.TotalPages > 0 && pResult.PageIndex > pResult.TotalPages) return NotFound();

            ViewData["Keyword"] = keyword;
            
            return View(pResult);
        }

        public async Task<IActionResult> DeleteAsync(int blog_id) {

            Blog item = await _context.Blogs.FindAsync(blog_id);

            // Not Found
            if(item == null) return NotFound();

            // Unauthorization
            if(item.AuthorUserName != User.Identity.Name) return BadRequest();

            _context.Blogs.Remove(item);
            await _context.SaveChangesAsync();


            return RedirectToAction("Index", "Profile", new {UserName=User.Identity.Name});

        }

        public async Task<IActionResult> NextAsync(int blog_id) {

            int MaxCount = (await _context.Blogs.OrderBy(item => item.Id).LastOrDefaultAsync()).Id;
            if(MaxCount == 0) return NotFound();
            while(true) {
                blog_id = blog_id % MaxCount + 1;
                Blog item = await _context.Blogs.FindAsync(blog_id);
                if(item != null) {
                    return RedirectToAction("Detail", new {blog_id=blog_id});
                }
            }

        }

        public async Task<IActionResult> PreviousAsync(int blog_id) {

            int MaxCount = (await _context.Blogs.OrderBy(item => item.Id).LastOrDefaultAsync()).Id;
            if(MaxCount == 0) return NotFound();
            while(true) {
                
                blog_id = MaxCount - (MaxCount + (1 - blog_id) % MaxCount) % MaxCount;
                Blog item = await _context.Blogs.FindAsync(blog_id);
                if(item != null) {
                    return RedirectToAction("Detail", new {blog_id=blog_id});
                }
            }

        }

    }
}