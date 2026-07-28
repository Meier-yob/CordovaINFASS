using CordovaINFASS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace CordovaINFASS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Temporary in-memory storage
        private static readonly List<User> Users = new();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => View();
        public IActionResult Login() => View();
        public IActionResult Register() => View();

        // POST: /Home/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }

            // Check if email already exists
            if (Users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "Email is already registered." }
                });
            }

            // 1. Map ViewModel to User Model
            var newUser = new User
            {
                Id = Users.Count + 1,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = model.Password, // In production, hash this password!
                CreatedAt = DateTime.UtcNow
            };

            // 2. Add model instance to in-memory list
            Users.Add(newUser);

            // 3. Generate SQL Query using the instance method
            string sqlQuery = newUser.ToInsertSqlQuery();

            return Json(new
            {
                success = true,
                sqlQuery = sqlQuery,
                redirectUrl = Url.Action("Login", "Home")
            });
        }

        // POST: /Home/Login// POST: /Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }

            var user = Users.FirstOrDefault(u =>
                u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash == model.Password);

            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "Invalid email or password." } });
            }

            // --- 🔑 ISSUE AUTHENTICATION COOKIE HERE ---
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Keeps user logged in across browser closes
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }
        //LOGOUT
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index","Home");
        }


        [HttpGet]
        public IActionResult GetAccountSettings()
        {
            // Read logged-in user ID directly from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "User session not found." });
            }

            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var model = new AccountSetting
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return Json(new { success = true, data = model });
        }           //update Usercreds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAccountSettings(AccountSetting model)
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, errors });
                }

                var user = Users.FirstOrDefault(u => u.Id == model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, errors = new[] { "User not found." } });
                }

                // Verify current password if user is updating password
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (user.PasswordHash != model.CurrentPassword)
                    {
                        return Json(new { success = false, errors = new[] { "Current password does not match." } });
                    }

                    user.PasswordHash = model.NewPassword;
                }

                // Update user profile fields
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                return Json(new { success = true, message = "Account details updated successfully!" });
            
       }
    }

}