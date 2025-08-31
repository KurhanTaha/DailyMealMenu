// Controllers/DessertsController.cs
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DailyMealMenu.Data;
using DailyMealMenu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DailyMealMenu.Controllers
{
    public class DessertsController : Controller
    {
        private readonly MealsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DessertsController(MealsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // INDEX: Yalnızca KATALOG kayıtları (MealId == null)
        public async Task<IActionResult> Index()
        {
            var desserts = await _context.Desserts
                .AsNoTracking()
                .Where(x => x.MealId == null)          // <-- kritik filtre
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(desserts);
        }

        // CREATE
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dessert dessert, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(dessert.ImageUrl))
                dessert.ImageUrl = null;

            // İsim tekilleştirme (katalog içinde)
            var nameKey = (dessert.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Desserts
                .AsNoTracking()
                .AnyAsync(x => x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
                ModelState.AddModelError(nameof(Dessert.Name), "Bu isimde bir tatlı zaten var (katalog). Lütfen farklı bir ad girin.");

            // Opsiyonel görsel yükleme
            if (image is { Length: > 0 })
            {
                var rel = await SaveImageAsync(image);
                if (rel != null) dessert.ImageUrl = rel;
            }

            if (!ModelState.IsValid) return View(dessert);

            dessert.CreatedAt = DateTime.Now;
            _context.Desserts.Add(dessert);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE GET
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();
            var dessert = await _context.Desserts.FindAsync(id.Value);
            if (dessert == null) return NotFound();
            return View(dessert);
        }

        // UPDATE POST (dosya yükleme + resmi kaldır)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([Bind("Id,Name,Ingredients,ImageUrl,Kcal,IsActive")] Dessert input,
                                                IFormFile? image, bool removeImage = false)
        {
            if (!ModelState.IsValid) return View(input);

            var entity = await _context.Desserts.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (entity == null) return NotFound();

            // Aynı isimli başka bir KATALOG kaydı var mı? (kendisi hariç)
            var nameKey = (input.Name ?? string.Empty).Trim().ToLower();
            var exists = await _context.Desserts
                .AsNoTracking()
                .AnyAsync(x => x.Id != input.Id &&
                               x.MealId == null &&
                               (x.Name ?? "").Trim().ToLower() == nameKey);

            if (exists)
            {
                ModelState.AddModelError(nameof(Dessert.Name), "Bu isimde bir tatlı zaten var (katalog).");
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

            // yeni görsel yüklendiyse
            if (image is { Length: > 0 })
            {
                TryDeletePhysicalFile(entity.ImageUrl);
                var rel = await SaveImageAsync(image);
                if (rel != null) entity.ImageUrl = rel;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var dessert = await _context.Desserts.FindAsync(id.Value);
            if (dessert == null) return NotFound();
            return View(dessert);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dessert = await _context.Desserts.FindAsync(id);
            if (dessert != null)
            {
                TryDeletePhysicalFile(dessert.ImageUrl);
                _context.Desserts.Remove(dessert);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------
        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("ImageUrl", "Lütfen geçerli bir resim dosyası yükleyin.");
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
            catch
            {
                // TODO: log
            }
        }
    }
}
