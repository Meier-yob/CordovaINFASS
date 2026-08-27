using CordovaINFASS.Data;
using CordovaINFASS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CordovaINFASS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext db, ILogger<AccountController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [Authorize]
        public IActionResult AccountPage() => View();

        [Authorize]
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetModelErrors() });
            }

            var emailTaken = await _db.Users.AnyAsync(u => u.Email == model.Email);
            if (emailTaken)
            {
                return Json(new { success = false, errors = new[] { "Email is already registered." } });
            }

            var newUser = new User
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim(),
                PasswordHash = model.Password,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                sqlQuery = newUser.ToInsertSqlQuery(),
                redirectUrl = Url.Action("Login", "Account")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetModelErrors() });
            }

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == model.Email && u.IsActive);

            if (user == null || user.PasswordHash != model.Password)
            {
                return Json(new { success = false, errors = new[] { "Invalid email or password." } });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 * 14 : 8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAccountSettings()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Json(new { success = false, message = "User session not found." });
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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccountSettings(AccountSetting model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetModelErrors() });
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || currentUser.Id != model.UserId)
            {
                return Json(new { success = false, errors = new[] { "User not found." } });
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "User not found." } });
            }

            var emailTaken = await _db.Users.AnyAsync(u =>
                u.Id != user.Id && u.Email == model.Email);
            if (emailTaken)
            {
                return Json(new { success = false, errors = new[] { "Email is already registered by another user." } });
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (user.PasswordHash != model.CurrentPassword)
                {
                    return Json(new { success = false, errors = new[] { "Current password does not match." } });
                }

                user.PasswordHash = model.NewPassword;
            }

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.Email = model.Email.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Account details updated successfully!" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var data = await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(new { success = true, data });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetModelErrors() });
            }

            var emailTaken = await _db.Users.AnyAsync(u => u.Email == model.Email);
            if (emailTaken)
            {
                return Json(new { success = false, errors = new[] { "Email is already registered." } });
            }

            var newUser = new User
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim(),
                PasswordHash = model.Password,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    newUser.Id,
                    newUser.FirstName,
                    newUser.LastName,
                    newUser.Email,
                    CreatedAt = newUser.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                }
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(int id, string firstName, string lastName, string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "User not found." } });
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, errors = new[] { "First name, last name and email are required." } });
            }

            var emailTaken = await _db.Users.AnyAsync(u =>
                u.Id != id && u.Email == email);
            if (emailTaken)
            {
                return Json(new { success = false, errors = new[] { "Email is already registered by another user." } });
            }

            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();
            user.Email = email.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = password;
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "User updated." });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "User not found." } });
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "User deleted." });
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }

            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        }

        private List<string> GetModelErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }
    }
}
