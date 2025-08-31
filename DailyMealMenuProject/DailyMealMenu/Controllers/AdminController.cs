using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyMealMenu.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
