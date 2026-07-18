using CordovaINFASS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CordovaINFASS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Add Context reference

        // Inject the database context here
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors = errors });
            }

            // Check if user already exists
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                return Json(new { success = false, errors = new[] { "Email is already registered." } });
            }

            // Map data to our Database Entity model
            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = model.Password // Real applications should hash this!
            };

            // Save records directly into LocalDB
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Json(new { success = true, redirectUrl = Url.Action("Login", "Home") });
        }

        // POST: /Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors = errors });
            }

            // Validate against existing records in LocalDB
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == model.Password);

            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "Invalid email or password." } });
            }

            // Authentication succeeded 
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }
    }
}