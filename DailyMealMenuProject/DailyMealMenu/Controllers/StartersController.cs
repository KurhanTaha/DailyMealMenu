// Controllers/StartersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Data;
using DailyMealMenu.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DailyMealMenu.Controllers
{
    public class StartersController : Controller
    {
        private readonly MealsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StartersController(MealsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // INDEX — sadece KATALOG (MealId == null) kayıtları listelenir
        public async Task<IActionResult> Index()
        {
            var starters = await _context.Starters
                .AsNoTracking()
                .Where(x => x.MealId == null)         // <-- kritik
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(starters);
        }

        // CREATE
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Ingredients,ImageUrl,Kcal,IsActive")] Starter starter,
                                                IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(starter.ImageUrl))
                starter.ImageUrl = null;

            // İSİM TEKİL: Katalogta aynı isim var mı?
            var nameKey = (starter.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Starters
                .AsNoTracking()
                .AnyAsync(x => x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
                ModelState.AddModelError(nameof(Starter.Name), "Bu isimde bir Başlangıç zaten var (katalog). Lütfen farklı bir ad giriniz.");

            // Opsiyonel görsel yükleme
            if (image is { Length: > 0 })
            {
                var rel = await SaveImageAsync(image);
                if (rel != null) starter.ImageUrl = rel;
            }

            if (!ModelState.IsValid) return View(starter);

            starter.CreatedAt = DateTime.Now;
            _context.Starters.Add(starter);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE GET
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();

            var starter = await _context.Starters.FindAsync(id.Value);
            if (starter == null) return NotFound();

            return View(starter);
        }

        // UPDATE POST (yeni görsel yükle / mevcut görseli kaldır) + isim tekilleştirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([Bind("Id,Name,Ingredients,ImageUrl,Kcal,IsActive")] Starter input,
                                                IFormFile? image, bool removeImage = false)
        {
            if (!ModelState.IsValid) return View(input);

            var entity = await _context.Starters.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (entity == null) return NotFound();

            // Aynı isimli başka bir KATALOG kaydı var mı? (kendisi hariç)
            var nameKey = (input.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Starters
                .AsNoTracking()
                .AnyAsync(x => x.Id != input.Id &&
                               x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
            {
                ModelState.AddModelError(nameof(Starter.Name), "Bu isimde bir Başlangıç zaten var (katalog).");
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

            // dosya seçilmediyse ve kaldır seçilmediyse, manuel girilen URL'yi uygula/boşsa null
            if (!removeImage && image == null)
                entity.ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl;

            await _context.SaveChangesAsync();
            TempData["Ok"] = "Kayıt güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var starter = await _context.Starters.FindAsync(id.Value);
            if (starter == null) return NotFound();
            return View(starter);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var starter = await _context.Starters.FindAsync(id);
            if (starter != null)
            {
                TryDeletePhysicalFile(starter.ImageUrl);
                _context.Starters.Remove(starter);
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
            catch { /* isteğe bağlı logla */ }
        }
    }
}
