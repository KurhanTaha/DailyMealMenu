using Microsoft.AspNetCore.Mvc;

namespace DailyMealMenu.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult YemekEkle()
        {
            return View();
        }
    }
}
