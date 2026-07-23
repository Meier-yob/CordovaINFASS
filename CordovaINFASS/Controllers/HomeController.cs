using System;
using System.Collections.Generic;
using System.Linq;
using CordovaINFASS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

                return Json(new { success = false, errors });
            }

            var user = Users.FirstOrDefault(u =>
                u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash == model.Password);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "Invalid email or password." }
                });
            }

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Index", "Home")
            });
        }
    }
}