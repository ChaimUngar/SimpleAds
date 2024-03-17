using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleAdsAuth.Data;
using SimpleAdsAuth.Web.Models;
using System.Security.Claims;

namespace SimpleAdsAuth.Web.Controllers
{
    public class AccountsController : Controller
    {
        private string _connectionString = "Data Source=.\\sqlexpress;Initial Catalog=SimpleAdsWeb; Integrated Security=True";

        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(Lister lister, string password)
        {
            var repo = new AdRepository(_connectionString);
            repo.AddLister(lister, password);
            return Redirect("/accounts/login");
        }

        public IActionResult Login()
        {
            if (TempData["message"] != null)
            {
                ViewBag.Message = (string)TempData["message"];
            }

            var lister = new Lister();
            if (User.Identity.IsAuthenticated)
            {
                var repo = new AdRepository(_connectionString);
                lister = repo.GetByEmail(User.Identity.Name);
            }

            return View(lister);
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var repo = new AdRepository(_connectionString);
            var lister = repo.Login(email, password);

            if (lister == null)
            {
                TempData["message"] = "Invalid Login!";
                return RedirectToAction("Login");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, email)
            };

            HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", ClaimTypes.Email, "roles"))).Wait();

            return Redirect("/home/newad");
        }

        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return Redirect("/home/index");
        }

        [Authorize]
        public IActionResult MyAccount()
        {
            var repo = new AdRepository(_connectionString);
            var vm = new MyAccountVM
            {
                Ads = repo.GetAdsByLister(User.Identity.Name)
            };
            return View(vm);
        }
    }
}

