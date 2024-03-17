using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleAdsAuth.Data;
using SimpleAdsAuth.Web.Models;
using System.Diagnostics;

namespace SimpleAdsAuth.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString = "Data Source=.\\sqlexpress;Initial Catalog=SimpleAdsWeb; Integrated Security=True";

        public IActionResult Index()
        {
            var repo = new AdRepository(_connectionString);
            List<Ad> ads = repo.GetAds();

            if (User.Identity.IsAuthenticated)
            {
                foreach (var a in ads)
                {

                    var lister = repo.GetByEmail(User.Identity.Name);

                    if (lister.Id == a.ListerId)
                    {
                        a.CanDelete = true;
                    }
                    else
                    {
                        a.CanDelete = false;
                    }
                }
            }
            return View(ads);
        }

        public IActionResult NewAd()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/accounts/login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult NewAd(Ad ad)
        {
            var repo = new AdRepository(_connectionString);
            var lister = repo.GetByEmail(User.Identity.Name);
            repo.AddAd(ad, lister.Email);
            return Redirect("/home/index");
        }

        [HttpPost]
        public IActionResult DeleteAd(int id)
        {
            var repo = new AdRepository(_connectionString);
            repo.DeleteAd(id);
            return Redirect("/");
        }
    }
}