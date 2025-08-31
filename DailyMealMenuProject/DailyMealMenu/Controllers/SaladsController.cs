// Controllers/SaladsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Data;
using DailyMealMenu.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DailyMealMenu.Controllers
{
    public class SaladsController : Controller
    {
        private readonly MealsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SaladsController(MealsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // INDEX — sadece KATALOG kayıtları (MealId == null) listelenir
        public async Task<IActionResult> Index()
        {
            var salads = await _context.Salads
                .AsNoTracking()
                .Where(x => x.MealId == null)   // <-- kritik filtre
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(salads);
        }

        // CREATE
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Ingredients,ImageUrl,Kcal,IsActive")] Salad salad,
                                                IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(salad.ImageUrl))
                salad.ImageUrl = null; // URL boşsa null

            // İSİM TEKİL: Katalogta aynı isim zaten var mı?
            var nameKey = (salad.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Salads
                .AsNoTracking()
                .AnyAsync(x => x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
                ModelState.AddModelError(nameof(Salad.Name), "Bu isimde bir Salata zaten var (katalog). Lütfen farklı bir ad girin.");

            // Opsiyonel dosya yükleme
            if (image is { Length: > 0 })
            {
                var rel = await SaveImageAsync(image);
                if (rel != null) salad.ImageUrl = rel;
            }

            if (!ModelState.IsValid) return View(salad);

            salad.CreatedAt = DateTime.Now;
            _context.Salads.Add(salad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE GET
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();

            var salad = await _context.Salads.FindAsync(id.Value);
            if (salad == null) return NotFound();

            return View(salad);
        }

        // UPDATE POST (yeni görsel yükle / mevcut görseli kaldır) + isim tekilleştirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([Bind("Id,Name,Ingredients,ImageUrl,Kcal,IsActive")] Salad input,
                                                IFormFile? image, bool removeImage = false)
        {
            if (!ModelState.IsValid) return View(input);

            var entity = await _context.Salads.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (entity == null) return NotFound();

            // Aynı isimli başka bir KATALOG kaydı var mı? (kendisi hariç)
            var nameKey = (input.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Salads
                .AsNoTracking()
                .AnyAsync(x => x.Id != input.Id &&
                               x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
            {
                ModelState.AddModelError(nameof(Salad.Name), "Bu isimde bir Salata zaten var (katalog).");
                return View(input);
            }

            // temel alanlar
            entity.Name = input.Name;
            entity.Ingredients = input.Ingredients;
            entity.Kcal = input.Kcal;
            entity.IsActive = input.IsActive;

            // görseli kaldır
            if (removeImage)
            {
                TryDeletePhysicalFile(entity.ImageUrl);
                entity.ImageUrl = null;
            }

            // yeni görsel
            if (image is { Length: > 0 })
            {
                TryDeletePhysicalFile(entity.ImageUrl);
                var rel = await SaveImageAsync(image);
                if (rel != null) entity.ImageUrl = rel;
            }

            // URL alanı manuel yazıldıysa (ve dosya seçilmediyse) onu da uygula
            if (!removeImage && image == null)
            {
                entity.ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? entity.ImageUrl : input.ImageUrl;
                if (string.IsNullOrWhiteSpace(input.ImageUrl)) entity.ImageUrl = null;
            }

            await _context.SaveChangesAsync();
            TempData["Ok"] = "Kayıt güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var salad = await _context.Salads.FindAsync(id.Value);
            if (salad == null) return NotFound();
            return View(salad);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salad = await _context.Salads.FindAsync(id);
            if (salad != null)
            {
                TryDeletePhysicalFile(salad.ImageUrl);
                _context.Salads.Remove(salad);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // -------- Helpers --------
        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("ImageUrl", "Lütfen bir resim dosyası yükleyin.");
                return null;
            }

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var folder = Path.Combine(webRoot, "uploads", "meals");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var physical = Path.Combine(folder, name);

            using (var fs = new FileStream(physical, FileMode.Create))
                await file.CopyToAsync(fs);

            return $"/uploads/meals/{name}";
        }

        private void TryDeletePhysicalFile(string? imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl)) return;

                var webRoot = _env.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRoot))
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                var physical = Path.Combine(webRoot, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(physical))
                    System.IO.File.Delete(physical);
            }
            catch { /* loglanabilir */ }
        }
    }
}
