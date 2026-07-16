using CordovaINFASS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq; // Required for handling Model state error arrays

namespace CordovaINFASS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors = errors });
            }

            // TODO: Add database validation and authentication logic here.
            // If details are incorrect, return:
            // return Json(new { success = false, errors = new[] { "Invalid email or password." } });

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        // GET: /Home/Logout
        public IActionResult Logout()
        {
            // TODO: clear authentication session/cookie here when auth is wired up
            return RedirectToAction("Login");
        }

        // GET: /Home/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
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

                return Json(new { success = false, errors = errors });
            }

            // TODO: Add database user creation / registration logic here.

            return Json(new { success = true, redirectUrl = Url.Action("Login", "Home") });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}