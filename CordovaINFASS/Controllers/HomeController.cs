using CordovaINFASS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        // New: Account page view
        public IActionResult AccountPage() => View();

        // Details view for an individual user
        public IActionResult UserDetails(int id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

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

            if (Users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "Email is already registered." }
                });
            }

            var newUser = new User
            {
                Id = Users.Count + 1,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = model.Password, // For demo only
                CreatedAt = DateTime.UtcNow
            };

            Users.Add(newUser);

            string sqlQuery = newUser.ToInsertSqlQuery();

            return Json(new
            {
                success = true,
                sqlQuery = sqlQuery,
                redirectUrl = Url.Action("Login", "Home")
            });
        }

        // POST: /Home/Login
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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        // LOGOUT
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        public IActionResult GetAccountSettings()
        {
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
        }

        // Update account settings for currently logged user
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

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (user.PasswordHash != model.CurrentPassword)
                {
                    return Json(new { success = false, errors = new[] { "Current password does not match." } });
                }

                user.PasswordHash = model.NewPassword;
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            return Json(new { success = true, message = "Account details updated successfully!" });
        }

        // --- API endpoints for AccountPage CRUD ---

        [HttpGet]
        public IActionResult GetUsers()
        {
            var data = Users
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToList();

            return Json(new { success = true, data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }

            if (Users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Json(new { success = false, errors = new[] { "Email is already registered." } });
            }

            var newUser = new User
            {
                Id = Users.Count + 1,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = model.Password,
                CreatedAt = DateTime.UtcNow
            };

            Users.Add(newUser);

            return Json(new
            {
                success = true,
                data = new { newUser.Id, newUser.FirstName, newUser.LastName, newUser.Email, CreatedAt = newUser.CreatedAt.ToString("yyyy-MM-dd HH:mm") }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUser(int id, string firstName, string lastName, string email, string password)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "User not found." } });
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, errors = new[] { "First name, last name and email are required." } });
            }

            if (!user.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                return Json(new { success = false, errors = new[] { "Email is already registered by another user." } });
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;

            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = password;
            }

            return Json(new { success = true, message = "User updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "User not found." } });
            }

            Users.Remove(user);
            return Json(new { success = true, message = "User deleted." });
        }
    }
}