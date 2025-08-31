// Controllers/OthersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Data;
using DailyMealMenu.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DailyMealMenu.Controllers
{
    public class OthersController : Controller
    {
        private readonly MealsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public OthersController(MealsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // INDEX — sadece KATALOG kayıtları (MealId == null) listelenir
        public async Task<IActionResult> Index()
        {
            var other = await _context.Others
                .AsNoTracking()
                .Where(x => x.MealId == null)     // <-- kritik filtre
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(other);
        }

        // CREATE
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Ingredients,ImageUrl,Kcal,IsActive")] Other other, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(other.ImageUrl))
                other.ImageUrl = null;

            // İSİM TEKİL: Katalogta aynı isim varsa engelle
            var nameKey = (other.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Others
                .AsNoTracking()
                .AnyAsync(x => x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
                ModelState.AddModelError(nameof(Other.Name), "Bu isimde bir kayıt (Diğer) zaten var (katalog). Lütfen farklı bir ad girin.");

            // Opsiyonel dosya
            if (image is { Length: > 0 })
            {
                var rel = await SaveImageAsync(image);
                if (rel != null) other.ImageUrl = rel;
            }

            if (!ModelState.IsValid) return View(other);

            other.CreatedAt = DateTime.Now;
            _context.Others.Add(other);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE GET
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();

            var other = await _context.Others.FindAsync(id.Value);
            if (other == null) return NotFound();

            return View(other);
        }

        // UPDATE POST (yeni görsel yükle / mevcut görseli kaldır + isim tekilleştirme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([Bind("Id,Name,Ingredients,ImageUrl,Kcal,IsActive")] Other input,
                                                IFormFile? image, bool removeImage = false)
        {
            if (!ModelState.IsValid) return View(input);

            var entity = await _context.Others.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (entity == null) return NotFound();

            // Aynı isimli başka bir KATALOG kaydı var mı? (kendisi hariç)
            var nameKey = (input.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Others
                .AsNoTracking()
                .AnyAsync(x => x.Id != input.Id &&
                               x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
            {
                ModelState.AddModelError(nameof(Other.Name), "Bu isimde bir kayıt (Diğer) zaten var (katalog).");
                return View(input);
            }

            // temel alanlar
            entity.Name = input.Name;
            entity.Ingredients = input.Ingredients;
            entity.Kcal = input.Kcal;
            entity.IsActive = input.IsActive;

            // mevcut görseli kaldır
            if (removeImage)
            {
                TryDeletePhysicalFile(entity.ImageUrl);
                entity.ImageUrl = null;
            }

            // yeni görsel geldiyse yükle, eskisini sil
            if (image is { Length: > 0 })
            {
                TryDeletePhysicalFile(entity.ImageUrl);
                var rel = await SaveImageAsync(image);
                if (rel != null) entity.ImageUrl = rel;
            }

            await _context.SaveChangesAsync();
            TempData["Ok"] = "Kayıt güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var other = await _context.Others.FindAsync(id.Value);
            if (other == null) return NotFound();

            return View(other);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var other = await _context.Others.FindAsync(id);
            if (other != null)
            {
                TryDeletePhysicalFile(other.ImageUrl);
                _context.Others.Remove(other);
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
