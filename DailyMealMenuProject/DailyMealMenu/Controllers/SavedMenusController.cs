using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Data;
using DailyMealMenu.Models;

namespace DailyMealMenu.Controllers
{
    [Authorize] // sadece giriş yapan görsün
    public class SavedMenusController : Controller
    {
        private readonly MealsDbContext _context;
        public SavedMenusController(MealsDbContext ctx) => _context = ctx;

        // Liste sayfası
        [HttpGet("/SavedMenus")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.MenuTemplates
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            // View'da okumak için parse edip yollayabiliriz
            var vm = list.Select(x => new SavedMenuVm
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                Title = x.Title,
                Items = SafeReadItems(x.ItemsJson)
            }).ToList();

            return View(vm); // Views/SavedMenus/Index.cshtml
        }

        // JSON dönen endpoint (YemekEkle otomatik doldurmak için)
        [HttpGet("/SavedMenus/Get")]
        public async Task<IActionResult> Get(int id)
        {
            var t = await _context.MenuTemplates.FindAsync(id);
            if (t == null) return NotFound(new { message = "Şablon bulunamadı." });

            var items = SafeReadItems(t.ItemsJson);
            return Json(new
            {
                id = t.Id,
                title = t.Title,
                createdAt = t.CreatedAt.ToString("s"),
                items
            });
        }

        // Sil
        [HttpPost("/SavedMenus/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _context.MenuTemplates.FindAsync(id);
            if (t != null)
            {
                _context.MenuTemplates.Remove(t);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Şablon silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private static List<TemplateItem> SafeReadItems(string json)
        {
            try
            {
                var items = JsonSerializer.Deserialize<List<TemplateItem>>(json) ?? new();
                return items;
            }
            catch
            {
                return new();
            }
        }
    }

    public class SavedMenuVm
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TemplateItem> Items { get; set; } = new();
    }
}
