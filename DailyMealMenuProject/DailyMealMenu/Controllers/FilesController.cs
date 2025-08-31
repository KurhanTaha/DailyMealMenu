using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace DailyMealMenu.Controllers
{
    public class FilesController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private const string MonthlyFileName = "monthly-menu.pdf";
        private readonly string _dataDir;
        private readonly string _pdfPath;

        public FilesController(IWebHostEnvironment env)
        {
            _env = env;
            _dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
            _pdfPath = Path.Combine(_dataDir, MonthlyFileName);
            Directory.CreateDirectory(_dataDir);
        }

        // PDF indir: /Files/Monthly
        [HttpGet("/Files/Monthly")]
        [AllowAnonymous]
        public IActionResult Monthly()
        {
            if (!System.IO.File.Exists(_pdfPath))
                return NotFound("Henüz aylık menü yüklenmemiş.");

            var fs = new FileStream(_pdfPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return File(fs, "application/pdf", "AylikMenu.pdf");
        }

        // PDF yükle: /Files/UploadMonthly
        // Büyük dosyalara izin ver (100 MB örnek)
        [Authorize]
        [HttpPost("/Files/UploadMonthly")]
        [RequestSizeLimit(104_857_600)] // 100 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMonthly(IFormFile file)
        {
            Console.WriteLine($"Gelen dosya: {(file == null ? "yok" : file.FileName)}, Boyut: {(file?.Length ?? 0)}");

            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["Err"] = "Lütfen bir PDF dosyası seçin.";
                    return RedirectToAction("YemekEkle", "Yemek");
                }

                var ext = Path.GetExtension(file.FileName);
                if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Err"] = "Sadece PDF yükleyebilirsiniz.";
                    return RedirectToAction("YemekEkle", "Yemek");
                }

                Directory.CreateDirectory(_dataDir);

                // Dosyayı kaydet
                using (var stream = new FileStream(_pdfPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                TempData["Ok"] = "Aylık menü PDF başarıyla yüklendi.";
                return RedirectToAction("YemekEkle", "Yemek");
            }
            catch (Exception ex)
            {
                // Tanı kolay olsun diye mesaj bırakalım
                TempData["Err"] = "PDF yükleme sırasında hata: " + ex.Message;
                return RedirectToAction("YemekEkle", "Yemek");
            }
        }
    }
}
