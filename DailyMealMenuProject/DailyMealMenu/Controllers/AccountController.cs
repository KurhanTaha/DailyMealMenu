using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace DailyMealMenu.Controllers
{
    public class AccountController : Controller
    {
        // örnek sabitler – istersen appsettings’ten oku
        private const string AdminUser = "taha";
        private const string AdminPass = "19070";

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            // basit kontrol – gerçek hayatta hash vs
            if (username == AdminUser && password == AdminPass)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Kalıcı cookie OLMASIN → IsPersistent = false (session cookie)
                // ExpiresUtc vermiyoruz; sürenin sahibi Program.cs’deki options.ExpireTimeSpan
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = false,     // tarayıcı oturumu
                    AllowRefresh = false
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("YemekEkle", "Yemek");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
