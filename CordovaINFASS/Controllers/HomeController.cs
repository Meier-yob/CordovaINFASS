using CordovaINFASS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CordovaINFASS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        public IActionResult Login() => RedirectToAction("Login", "Account");

        public IActionResult Register() => RedirectToAction("Register", "Account");

        public IActionResult AccountPage() => RedirectToAction("AccountPage", "Account");

        public IActionResult UserDetails(int id) => RedirectToAction("UserDetails", "Account", new { id });

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
