using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Data;
using DailyMealMenu.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;

namespace DailyMealMenu.Controllers
{
    public class SoupsController : Controller
    {
        private readonly MealsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SoupsController(MealsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // LIST: Yalnızca KATALOG kayıtları (MealId == null)
        public async Task<IActionResult> Index()
        {
            var soups = await _context.Soups
                .AsNoTracking()
                .Where(x => x.MealId == null)           // <-- kritik filtre
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(soups);
        }

        // CREATE (opsiyonel upload)
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Ingredients,ImageUrl,Kcal,IsActive")] Soup soup, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(soup.ImageUrl)) soup.ImageUrl = null;

            // --- Aynı isimli KATALOG kaydı zaten var mı? (case-insensitive + trim) ---
            var nameKey = (soup.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Soups
                .AsNoTracking()
                .AnyAsync(x => x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
            {
                ModelState.AddModelError(nameof(Soup.Name), "Bu isimde bir çorba zaten var (katalog). Lütfen farklı bir ad girin.");
            }

            if (image is { Length: > 0 })
            {
                var rel = await SaveImageAsync(image);
                if (rel != null) soup.ImageUrl = rel;
            }

            if (!ModelState.IsValid) return View(soup);

            soup.CreatedAt = DateTime.Now;
            _context.Soups.Add(soup);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE GET
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();
            var soup = await _context.Soups.FindAsync(id.Value);
            if (soup == null) return NotFound();
            return View(soup);
        }

        // UPDATE POST (dosya yükleme + resmi kaldır)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([Bind("Id,Name,Ingredients,ImageUrl,Kcal,IsActive")] Soup input, IFormFile? image, bool removeImage = false)
        {
            if (!ModelState.IsValid) return View(input);

            var entity = await _context.Soups.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (entity == null) return NotFound();

            // --- Aynı isimli başka bir KATALOG kaydı var mı? (kendisi hariç) ---
            var nameKey = (input.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Soups
                .AsNoTracking()
                .AnyAsync(x => x.Id != input.Id &&
                               x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
            {
                ModelState.AddModelError(nameof(Soup.Name), "Bu isimde bir çorba zaten var (katalog).");
                return View(input);
            }

            // temel alanlar
            entity.Name = input.Name;
            entity.Ingredients = input.Ingredients;
            entity.Kcal = input.Kcal;
            entity.IsActive = input.IsActive;

            // mevcut resmi kaldır
            if (removeImage)
            {
                TryDeletePhysicalFile(entity.ImageUrl);
                entity.ImageUrl = null;
            }

            // yeni resim yüklendiyse
            if (image is { Length: > 0 })
            {
                TryDeletePhysicalFile(entity.ImageUrl);
                var rel = await SaveImageAsync(image);
                if (rel != null) entity.ImageUrl = rel;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var soup = await _context.Soups.FindAsync(id.Value);
            if (soup == null) return NotFound();
            return View(soup);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var soup = await _context.Soups.FindAsync(id);
            if (soup != null)
            {
                TryDeletePhysicalFile(soup.ImageUrl);
                _context.Soups.Remove(soup);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------

        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("ImageUrl", "Lütfen geçerli bir resim dosyası yükleyiniz.");
                return null;
            }

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var folder = Path.Combine(webRoot, "uploads", "meals");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var physicalPath = Path.Combine(folder, name);

            using (var fs = new FileStream(physicalPath, FileMode.Create))
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
            catch { /* loglayabilirsin */ }
        }
    }
}
