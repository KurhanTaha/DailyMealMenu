using Microsoft.AspNetCore.Mvc;

namespace DailyMealMenu.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public UploadController(IWebHostEnvironment env) { _env = env; }

        // İframe sayfası (form burada)
        [HttpGet]
        public IActionResult Image(string field = "ImageUrl")
        {
            ViewBag.Field = field; // parent formdaki hangi input doldurulacak
            return View();
        }

        // Dosyayı yükle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Image(IFormFile file, string field = "ImageUrl")
        {
            if (file == null || file.Length == 0)
                return Content("<script>window.parent.postMessage({ ok:false, msg:'Dosya yok' }, '*');</script>", "text/html");

            // Boyut (ör: 2 MB)
            const long maxBytes = 2 * 1024 * 1024;
            if (file.Length > maxBytes)
                return Content("<script>window.parent.postMessage({ ok:false, msg:'Maksimum 2MB' }, '*');</script>", "text/html");

            // Tip kontrolü
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return Content("<script>window.parent.postMessage({ ok:false, msg:'Sadece JPG/PNG/WebP' }, '*');</script>", "text/html");

            // Klasör
            var uploadRel = Path.Combine("uploads", "images");
            var uploadAbs = Path.Combine(_env.WebRootPath ?? "wwwroot", uploadRel);
            Directory.CreateDirectory(uploadAbs);

            // Benzersiz isim
            var name = $"{Guid.NewGuid():N}{ext}";
            var saveAbs = Path.Combine(uploadAbs, name);
            using (var fs = new FileStream(saveAbs, FileMode.Create))
                file.CopyTo(fs);

            var url = "/" + Path.Combine(uploadRel, name).Replace("\\", "/");

            // Parent’e mesaj gönder (input’u dolduracağız)
            var html = $@"<script>
                window.parent.postMessage({{ ok:true, field: '{field}', url: '{url}' }}, '*');
            </script>";
            return Content(html, "text/html");
        }
    }
}
