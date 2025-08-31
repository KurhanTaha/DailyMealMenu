using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Data;
using DailyMealMenu.Models;

namespace DailyMealMenu.Controllers
{
    // === Menü kaydetme isteği için DTO'lar ===
    public class SaveMenuRequest
    {
        [Required]
        public string tarih { get; set; } = default!; // "yyyy-MM-dd"

        [Required]
        public List<SaveMenuItem> yemekler { get; set; } = new();
    }

    public class SaveMenuItem
    {
        public int yemekId { get; set; }
        public string kategori { get; set; } = ""; // soups, maindishes, desserts, salads, starters, others
    }

    public class MealsController : Controller
    {
        private readonly MealsDbContext _context;

        public MealsController(MealsDbContext context)
        {
            _context = context;
        }

        // GET: Meals
        public async Task<IActionResult> Index()
        {
            var list = await _context.Meals
                .Include(m => m.Soups)
                .Include(m => m.MainDishes)
                .Include(m => m.Desserts)
                .Include(m => m.Salads)
                .Include(m => m.Starters)
                .Include(m => m.Others)
                .OrderByDescending(m => m.Date)
                .Select(m => new MealListItemVm
                {
                    Id = m.Id,
                    Date = m.Date,
                    Soups = m.Soups.Select(x => x.Name).ToList(),
                    MainDishes = m.MainDishes.Select(x => x.Name).ToList(),
                    Desserts = m.Desserts.Select(x => x.Name).ToList(),
                    Salads = m.Salads.Select(x => x.Name).ToList(),
                    Starters = m.Starters.Select(x => x.Name).ToList(),
                    Others = m.Others.Select(x => x.Name).ToList()
                })
                .ToListAsync();

            return View(list); // Views/Meals/Index.cshtml
        }

        // GET: Meals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var meal = await _context.Meals
                .Include(m => m.MainDishes)
                .Include(m => m.Soups)
                .Include(m => m.Desserts)
                .Include(m => m.Salads)
                .Include(m => m.Starters)
                .Include(m => m.Others)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null)
                return NotFound();

            return View(meal);
        }

        // GET: Meals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Meals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date")] Meal meal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(meal);
        }

        // GET: Meals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var meal = await _context.Meals.FindAsync(id);
            if (meal == null)
                return NotFound();

            return View(meal);
        }

        // POST: Meals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date")] Meal meal)
        {
            if (id != meal.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MealExists(meal.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(meal);
        }

        // GET: Meals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var meal = await _context.Meals
                .Include(m => m.Soups)
                .Include(m => m.MainDishes)
                .Include(m => m.Desserts)
                .Include(m => m.Salads)
                .Include(m => m.Starters)
                .Include(m => m.Others)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null) return NotFound();

            var vm = new MealListItemVm
            {
                Id = meal.Id,
                Date = meal.Date,
                SoupsCount = meal.Soups.Count,
                MainDishesCount = meal.MainDishes.Count,
                DessertsCount = meal.Desserts.Count,
                SaladsCount = meal.Salads.Count,
                StartersCount = meal.Starters.Count,
                OthersCount = meal.Others.Count
            };

            return View(vm); // Views/Meals/Delete.cshtml
        }

        // POST: /Meals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meal = await _context.Meals
                .Include(m => m.Soups)
                .Include(m => m.MainDishes)
                .Include(m => m.Desserts)
                .Include(m => m.Salads)
                .Include(m => m.Starters)
                .Include(m => m.Others)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null) return NotFound();

            meal.Soups?.Clear();
            meal.MainDishes?.Clear();
            meal.Desserts?.Clear();
            meal.Salads?.Clear();
            meal.Starters?.Clear();
            meal.Others?.Clear();

            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Menü silindi.";
            return RedirectToAction(nameof(Index));
        }

        private bool MealExists(int id)
        {
            return _context.Meals.Any(e => e.Id == id);
        }

        // ✅ API endpoint (AJAX ile menüyü göstermek için)
        [HttpGet("api/meals/by-date")]
        public async Task<IActionResult> GetMealByDate([FromQuery] string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return BadRequest(new { message = "Tarih parametresi gerekli (yyyy-MM-dd)." });

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return BadRequest(new { message = "Tarih formatı yanlış. Örn: 2025-08-15" });

            var meal = await _context.Meals
                .Include(m => m.MainDishes)
                .Include(m => m.Soups)
                .Include(m => m.Desserts)
                .Include(m => m.Salads)
                .Include(m => m.Starters)
                .Include(m => m.Others)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Date.Date == parsedDate.Date);

            if (meal == null)
                return NotFound(new { message = "Bu tarihe ait menü bulunamadı." });

            var dto = new
            {
                date = parsedDate.ToString("yyyy-MM-dd"),
                soups = meal.Soups.Select(x => new { id = x.Id, name = x.Name, kcal = x.Kcal, ingredients = x.Ingredients, imageUrl = x.ImageUrl }),
                mainDishes = meal.MainDishes.Select(x => new { id = x.Id, name = x.Name, kcal = x.Kcal, ingredients = x.Ingredients, imageUrl = x.ImageUrl }),
                desserts = meal.Desserts.Select(x => new { id = x.Id, name = x.Name, kcal = x.Kcal, ingredients = x.Ingredients, imageUrl = x.ImageUrl }),
                salads = meal.Salads.Select(x => new { id = x.Id, name = x.Name, kcal = x.Kcal, ingredients = x.Ingredients, imageUrl = x.ImageUrl }),
                starters = meal.Starters.Select(x => new { id = x.Id, name = x.Name, kcal = x.Kcal, ingredients = x.Ingredients, imageUrl = x.ImageUrl }),
                others = meal.Others.Select(x => new { id = x.Id, name = x.Name, kcal = x.Kcal, ingredients = x.Ingredients, imageUrl = x.ImageUrl })
            };
            return Json(dto);
        }

        // === GÜNLÜK MENÜYÜ KAYDET (KATALOGTAN KLONLAYARAK) ===
        // Frontend: POST /Yemek/MenuKaydet  body: { tarih:"yyyy-MM-dd", yemekler:[{yemekId, kategori}, ...] }
        [HttpPost("/Yemek/MenuKaydet")]
        public async Task<IActionResult> SaveMenu([FromBody] SaveMenuRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Geçersiz istek. ❌" });

            if (!DateTime.TryParseExact(req.tarih, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return BadRequest(new { message = "Tarih formatı yanlış. Örn: 2025-08-15 ❌" });

            if (req.yemekler == null || req.yemekler.Count == 0)
                return BadRequest(new { message = "En az bir yemek seçiniz. ❌" });

            if (req.yemekler.Count > 4)
                return BadRequest(new { message = "En fazla 4 yemek seçebilirsiniz. ❌" });

            // ⛔ Aynı güne daha önce menü atanmış mı?
            var existingMenu = await _context.Meals
                .AsNoTracking()
                .AnyAsync(m => m.Date.Date == date.Date);

            if (existingMenu)
                return BadRequest(new { message = "Bu güne zaten menü atanmış. ❌" });

            // ✅ Menü yoksa yeni oluştur
            var meal = new Meal { Date = date };
            _context.Meals.Add(meal);
            await _context.SaveChangesAsync(); // meal.Id lazım

            // Duplicate’leri engellemek için id setleri
            var soupSet = new HashSet<int>();
            var mainSet = new HashSet<int>();
            var dessertSet = new HashSet<int>();
            var saladSet = new HashSet<int>();
            var starterSet = new HashSet<int>();
            var otherSet = new HashSet<int>();

            foreach (var s in req.yemekler)
            {
                var kat = (s.kategori ?? "").Trim().ToLowerInvariant();
                var id = s.yemekId;

                switch (kat)
                {
                    case "soups":
                        if (soupSet.Add(id))
                        {
                            var src = await _context.Soups.AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == id && x.MealId == null);
                            if (src != null)
                                _context.Soups.Add(new Soup
                                {
                                    Name = src.Name,
                                    Ingredients = src.Ingredients,
                                    Kcal = src.Kcal,
                                    ImageUrl = src.ImageUrl,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    MealId = meal.Id
                                });
                        }
                        break;

                    case "maindishes":
                    case "maindish":
                        if (mainSet.Add(id))
                        {
                            var src = await _context.MainDishes.AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == id && x.MealId == null);
                            if (src != null)
                                _context.MainDishes.Add(new MainDish
                                {
                                    Name = src.Name,
                                    Ingredients = src.Ingredients,
                                    Kcal = src.Kcal,
                                    ImageUrl = src.ImageUrl,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    MealId = meal.Id
                                });
                        }
                        break;

                    case "desserts":
                        if (dessertSet.Add(id))
                        {
                            var src = await _context.Desserts.AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == id && x.MealId == null);
                            if (src != null)
                                _context.Desserts.Add(new Dessert
                                {
                                    Name = src.Name,
                                    Ingredients = src.Ingredients,
                                    Kcal = src.Kcal,
                                    ImageUrl = src.ImageUrl,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    MealId = meal.Id
                                });
                        }
                        break;

                    case "salads":
                        if (saladSet.Add(id))
                        {
                            var src = await _context.Salads.AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == id && x.MealId == null);
                            if (src != null)
                                _context.Salads.Add(new Salad
                                {
                                    Name = src.Name,
                                    Ingredients = src.Ingredients,
                                    Kcal = src.Kcal,
                                    ImageUrl = src.ImageUrl,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    MealId = meal.Id
                                });
                        }
                        break;

                    case "starters":
                        if (starterSet.Add(id))
                        {
                            var src = await _context.Starters.AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == id && x.MealId == null);
                            if (src != null)
                                _context.Starters.Add(new Starter
                                {
                                    Name = src.Name,
                                    Ingredients = src.Ingredients,
                                    Kcal = src.Kcal,
                                    ImageUrl = src.ImageUrl,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    MealId = meal.Id
                                });
                        }
                        break;

                    case "others":
                        if (otherSet.Add(id))
                        {
                            var src = await _context.Others.AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == id && x.MealId == null);
                            if (src != null)
                                _context.Others.Add(new Other
                                {
                                    Name = src.Name,
                                    Ingredients = src.Ingredients,
                                    Kcal = src.Kcal,
                                    ImageUrl = src.ImageUrl,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    MealId = meal.Id
                                });
                        }
                        break;

                    default:
                        // bilinmeyen kategori gelirse geç
                        break;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Menü başarıyla kaydedildi." });
        }

        // =========================
        //  MENÜYÜ ŞABLON OLARAK KAYDET
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAsTemplate(int id)
        {
            var meal = await _context.Meals
                .Include(m => m.Soups)
                .Include(m => m.MainDishes)
                .Include(m => m.Desserts)
                .Include(m => m.Salads)
                .Include(m => m.Starters)
                .Include(m => m.Others)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null)
            {
                TempData["Err"] = "Menü bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var items = new List<TemplateItem>();

            void PushRange(string kategori, IEnumerable<string>? names)
            {
                if (names == null) return;

                foreach (var name in names)
                {
                    if (items.Count >= 4) break;

                    int? catalogId = null;

                    switch (kategori)
                    {
                        case "soups":
                            catalogId = _context.Soups
                                .Where(s => s.MealId == null && s.Name == name)
                                .Select(s => (int?)s.Id)
                                .FirstOrDefault();
                            break;

                        case "maindishes":
                            catalogId = _context.MainDishes
                                .Where(s => s.MealId == null && s.Name == name)
                                .Select(s => (int?)s.Id)
                                .FirstOrDefault();
                            break;

                        case "desserts":
                            catalogId = _context.Desserts
                                .Where(s => s.MealId == null && s.Name == name)
                                .Select(s => (int?)s.Id)
                                .FirstOrDefault();
                            break;

                        case "salads":
                            catalogId = _context.Salads
                                .Where(s => s.MealId == null && s.Name == name)
                                .Select(s => (int?)s.Id)
                                .FirstOrDefault();
                            break;

                        case "starters":
                            catalogId = _context.Starters
                                .Where(s => s.MealId == null && s.Name == name)
                                .Select(s => (int?)s.Id)
                                .FirstOrDefault();
                            break;

                        case "others":
                            catalogId = _context.Others
                                .Where(s => s.MealId == null && s.Name == name)
                                .Select(s => (int?)s.Id)
                                .FirstOrDefault();
                            break;
                    }

                    items.Add(new TemplateItem
                    {
                        kategori = kategori,
                        name = name,
                        catalogId = catalogId
                    });
                }
            }

            PushRange("soups", meal.Soups?.Select(s => s.Name));
            PushRange("maindishes", meal.MainDishes?.Select(s => s.Name));
            PushRange("desserts", meal.Desserts?.Select(s => s.Name));
            PushRange("salads", meal.Salads?.Select(s => s.Name));
            PushRange("starters", meal.Starters?.Select(s => s.Name));
            PushRange("others", meal.Others?.Select(s => s.Name));

            if (items.Count == 0)
            {
                TempData["Err"] = "Bu günde kayıtlı yemek bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var template = new MenuTemplate
            {
                ItemsJson = JsonSerializer.Serialize(items),
                CreatedAt = DateTime.Now,
                Title = meal.Date.ToString("dd.MM.yyyy") + " menüsü"
            };

            _context.MenuTemplates.Add(template);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Menü şablon olarak kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        // === KAYITLI MENÜ ŞABLONLARI ===
        [HttpGet]
        public async Task<IActionResult> Templates()
        {
            var list = await _context.MenuTemplates
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(list); // Views/Meals/Templates.cshtml
        }

        // === BİR ŞABLONUN ÖĞELERİNİ JSON DÖN ===
        [HttpGet]
        public async Task<IActionResult> GetTemplateItems(int id)
        {
            var tpl = await _context.MenuTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tpl == null) return NotFound();

            return Content(tpl.ItemsJson, "application/json");
        }
    }
}
