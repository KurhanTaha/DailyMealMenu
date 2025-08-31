using DailyMealMenu.Data;
using DailyMealMenu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DailyMealMenu.Controllers
{
    public class YemekController : Controller
    {
        private readonly MealsDbContext _context;

        public YemekController(MealsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public IActionResult YemekEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult YemekEkle(YemekModel model)
        {
            // Burada kendi ekleme mantığınız varsa ekleyebilirsiniz
            return Json(new { success = true, message = "Yemek eklendi!" });
        }

        [HttpGet]
        [Authorize]
        // GET: YEMEK OLUŞTUR
        public IActionResult YemekOlustur()
        {
            return View();
        }

        // POST: YEMEK OLUŞTUR
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult YemekOlustur(YeniYemekModel yemek)
        {
            // ImageUrl opsiyonel olsun:
            if (string.IsNullOrWhiteSpace(yemek.ImageUrl))
            {
                yemek.ImageUrl = null;
                // ViewModel'de [Required] olsa bile bu satır validasyon hatasını düşürür
                ModelState.Remove(nameof(YeniYemekModel.ImageUrl));
            }

            if (!ModelState.IsValid)
            {
                return View(yemek);
            }

            // Ortak alanları
            var now = DateTime.Now;

            switch (yemek.Kategori)
            {
                case "Soup":
                    _context.Soups.Add(new Soup
                    {
                        Name = yemek.Name,
                        Ingredients = yemek.Ingredients,
                        Kcal = yemek.Kcal,
                        ImageUrl = yemek.ImageUrl, // null olabilir
                        CreatedAt = now,
                        IsActive = true
                    });
                    break;

                case "Starter":
                    _context.Starters.Add(new Starter
                    {
                        Name = yemek.Name,
                        Ingredients = yemek.Ingredients,
                        Kcal = yemek.Kcal,
                        ImageUrl = yemek.ImageUrl,
                        CreatedAt = now,
                        IsActive = true
                    });
                    break;

                case "Salad":
                    _context.Salads.Add(new Salad
                    {
                        Name = yemek.Name,
                        Ingredients = yemek.Ingredients,
                        Kcal = yemek.Kcal,
                        ImageUrl = yemek.ImageUrl,
                        CreatedAt = now,
                        IsActive = true
                    });
                    break;

                case "Other":
                    _context.Others.Add(new Other
                    {
                        Name = yemek.Name,
                        Ingredients = yemek.Ingredients,
                        Kcal = yemek.Kcal,
                        ImageUrl = yemek.ImageUrl,
                        CreatedAt = now,
                        IsActive = true
                    });
                    break;

                case "MainDish":
                    _context.MainDishes.Add(new MainDish
                    {
                        Name = yemek.Name,
                        Ingredients = yemek.Ingredients,
                        Kcal = yemek.Kcal,
                        ImageUrl = yemek.ImageUrl,
                        CreatedAt = now,
                        IsActive = true
                    });
                    break;

                case "Dessert":
                    _context.Desserts.Add(new Dessert
                    {
                        Name = yemek.Name,
                        Ingredients = yemek.Ingredients,
                        Kcal = yemek.Kcal,
                        ImageUrl = yemek.ImageUrl,
                        CreatedAt = now,
                        IsActive = true
                    });
                    break;

                default:
                    // Beklenmeyen kategori: formu geri göster
                    return View(yemek);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(YemekEkle));
        }

        // ================================
        //    MENÜ KAYDET (AJAX POST)
        // ================================
        [HttpPost]
        public async Task<IActionResult> MenuKaydet([FromBody] MenuKaydetDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Tarih) || dto.Yemekler == null)
                return BadRequest(new { success = false, message = "Eksik veri." });

            if (dto.Yemekler.Count != 4)
                return BadRequest(new { success = false, message = "Tam olarak 4 yemek seçmelisiniz." });

            if (!DateTime.TryParse(dto.Tarih, out var date))
                return BadRequest(new { success = false, message = "Tarih formatı hatalı." });

            // 1) O tarihin Meal'ını bul/oluştur
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Date == date);
            if (meal == null)
            {
                meal = new Meal { Date = date };
                _context.Meals.Add(meal);
                await _context.SaveChangesAsync(); // Id üretmek için
            }

            // 2) (Opsiyonel) Aynı güne daha önce menü girildiyse engelle
            var mevcutVar =
                   await _context.Soups.AnyAsync(s => s.MealId == meal.Id)
                || await _context.MainDishes.AnyAsync(s => s.MealId == meal.Id)
                || await _context.Desserts.AnyAsync(s => s.MealId == meal.Id)
                || await _context.Salads.AnyAsync(s => s.MealId == meal.Id)
                || await _context.Starters.AnyAsync(s => s.MealId == meal.Id)
                || await _context.Others.AnyAsync(s => s.MealId == meal.Id);

            if (mevcutVar)
                return Conflict(new { success = false, message = "Bu tarih için menü zaten oluşturulmuş." });

            // 3) Seçilen her yemeği, katalog kaydını BOZMADAN klonla ve MealId ile ilişkilendir
            foreach (var item in dto.Yemekler)
            {
                switch (item.Kategori)
                {
                    case "soups":
                        {
                            var src = await _context.Soups.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.YemekId);
                            if (src == null) return NotFound(new { success = false, message = "Çorba bulunamadı." });

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
                            break;
                        }
                    case "mainDishes":
                        {
                            var src = await _context.MainDishes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.YemekId);
                            if (src == null) return NotFound(new { success = false, message = "Ana yemek bulunamadı." });

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
                            break;
                        }
                    case "desserts":
                        {
                            var src = await _context.Desserts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.YemekId);
                            if (src == null) return NotFound(new { success = false, message = "Tatlı bulunamadı." });

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
                            break;
                        }
                    case "salads":
                        {
                            var src = await _context.Salads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.YemekId);
                            if (src == null) return NotFound(new { success = false, message = "Salata bulunamadı." });

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
                            break;
                        }
                    case "starters":
                        {
                            var src = await _context.Starters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.YemekId);
                            if (src == null) return NotFound(new { success = false, message = "Başlangıç bulunamadı." });

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
                            break;
                        }
                    case "others":
                        {
                            var src = await _context.Others.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.YemekId);
                            if (src == null) return NotFound(new { success = false, message = "Diğer yemek bulunamadı." });

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
                            break;
                        }
                    default:
                        return BadRequest(new { success = false, message = $"Geçersiz kategori: {item.Kategori}" });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Menü başarıyla kaydedildi.", mealId = meal.Id });
        }
    }
}
