using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Data;

namespace DailyMealMenu.Controllers
{
    [ApiController]
    [Route("api/yemekler")]
    public class YemeklerApiController : ControllerBase
    {
        private readonly MealsDbContext _context;
        public YemeklerApiController(MealsDbContext context) => _context = context;

        // kategori: soups | maindishes | desserts | salads | starters | others
        [HttpGet("{kategori}")]
        public async Task<IActionResult> GetByCategory(string kategori)
        {
            if (string.IsNullOrWhiteSpace(kategori))
                return BadRequest(new { message = "Kategori gerekli." });

            var key = kategori.Trim().ToLowerInvariant();

            switch (key)
            {
                case "soups":
                    {
                        var list = await _context.Soups
                            .AsNoTracking()
                            .Where(x => x.MealId == null && x.IsActive) // sadece katalog + aktif
                            .GroupBy(x => (x.Name ?? "").Trim())
                            .Select(g => new { id = g.Min(x => x.Id), name = g.Key })
                            .OrderBy(x => x.name)
                            .ToListAsync();
                        return Ok(list);
                    }

                case "maindishes":
                    {
                        var list = await _context.MainDishes
                            .AsNoTracking()
                            .Where(x => x.MealId == null && x.IsActive)
                            .GroupBy(x => (x.Name ?? "").Trim())
                            .Select(g => new { id = g.Min(x => x.Id), name = g.Key })
                            .OrderBy(x => x.name)
                            .ToListAsync();
                        return Ok(list);
                    }

                case "desserts":
                    {
                        var list = await _context.Desserts
                            .AsNoTracking()
                            .Where(x => x.MealId == null && x.IsActive)
                            .GroupBy(x => (x.Name ?? "").Trim())
                            .Select(g => new { id = g.Min(x => x.Id), name = g.Key })
                            .OrderBy(x => x.name)
                            .ToListAsync();
                        return Ok(list);
                    }

                case "salads":
                    {
                        var list = await _context.Salads
                            .AsNoTracking()
                            .Where(x => x.MealId == null && x.IsActive)
                            .GroupBy(x => (x.Name ?? "").Trim())
                            .Select(g => new { id = g.Min(x => x.Id), name = g.Key })
                            .OrderBy(x => x.name)
                            .ToListAsync();
                        return Ok(list);
                    }

                case "starters":
                    {
                        var list = await _context.Starters
                            .AsNoTracking()
                            .Where(x => x.MealId == null && x.IsActive)
                            .GroupBy(x => (x.Name ?? "").Trim())
                            .Select(g => new { id = g.Min(x => x.Id), name = g.Key })
                            .OrderBy(x => x.name)
                            .ToListAsync();
                        return Ok(list);
                    }

                case "others":
                    {
                        var list = await _context.Others
                            .AsNoTracking()
                            .Where(x => x.MealId == null && x.IsActive)
                            .GroupBy(x => (x.Name ?? "").Trim())
                            .Select(g => new { id = g.Min(x => x.Id), name = g.Key })
                            .OrderBy(x => x.name)
                            .ToListAsync();
                        return Ok(list);
                    }

                default:
                    return NotFound(new { message = "Geçersiz kategori. Geçerli değerler: soups, maindishes, desserts, salads, starters, others." });
            }
        }
    }
}
