using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlogWebMVCIdentityAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using BlogWebMVCIdentityAuth.Models.Views;

namespace BlogWebMVCIdentityAuth.Controllers
{
    public class IdentityController : Controller
    {

        readonly UserManager<Author> _userManager;
        readonly SignInManager<Author> _signInManager;
        readonly ILogger<IdentityController> _logger;

        public IdentityController(ILogger<IdentityController> logger, UserManager<Author> userManager, SignInManager<Author> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            
            return View();
        }

        [HttpGet("Register")]
        public IActionResult Register(){
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(LoginViewModel LoginViewModel, string returnUrl = null){

            System.Console.WriteLine("Login Post...");

            if(ModelState.IsValid) {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, 
                // set lockoutOnFailure: true

                var result = await _signInManager.PasswordSignInAsync(LoginViewModel.Username,
                           LoginViewModel.Password, LoginViewModel.remember, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {LoginViewModel.Username} logged in.");
                    returnUrl = returnUrl ?? Url.Content($"~/{LoginViewModel.Username}");
                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    string errorMessage = $"User {LoginViewModel.Username} account locked out in 30 seconds.";
                    _logger.LogWarning(errorMessage);
                    TempData["ErrorMessage"] = errorMessage;
                    return RedirectToAction("Login");
                }

                TempData["ErrorMessage"] = "Username or Password invalid. Login unsuccessfully. Try again";
            }

            return View(LoginViewModel);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel oRegisterViewModel)
        {
            // System.Console.WriteLine("Register Post...");

            if (ModelState.IsValid)
            {   
                List<String> DataValidators = new List<string>();
                Author user = (Author) oRegisterViewModel;
                IdentityResult result = await _userManager.CreateAsync(user, oRegisterViewModel.Password);
                if (result.Succeeded) {
                    
                    string message = $"Your account '{user.UserName}' created successfully.";
                    _logger.LogInformation(message);

                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    DataValidators.Add(error.Description);
                }

                ViewData["DataValidators"] = DataValidators;
            }
            
            return View(oRegisterViewModel);

        }

        [HttpGet("Logout")]
        public async Task<IActionResult> LogoutAsync(string returnUrl = null) {

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            returnUrl = returnUrl ?? Url.Content("~/");

            return LocalRedirect(returnUrl);
        }
    }
}