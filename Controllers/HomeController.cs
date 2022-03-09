using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlogWebMVCIdentityAuth.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlogWebMVCIdentityAuth.Data;
using BlogWebMVCIdentityAuth.Models.Views;

namespace BlogWebMVCIdentityAuth.Controllers
{

    public class HomeController : Controller
    {
        readonly ILogger<HomeController> _logger;
        readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) {
            _logger  = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Contact")]
        public IActionResult Contact()
        {
            IEnumerable<Blog> RecentPosts = _context.Blogs.OrderByDescending(item => item.DateTimeOfPublic).Take(3).ToList();
            ViewData["RecentPosts"] = RecentPosts;
            return View();
        }

        [HttpGet("AboutUs")]
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            string  RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(new ErrorViewModel { RequestId=RequestId });
        }
    }
}
