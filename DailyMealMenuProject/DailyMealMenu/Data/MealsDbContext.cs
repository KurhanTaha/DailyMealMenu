using Microsoft.EntityFrameworkCore;
using DailyMealMenu.Models;



namespace DailyMealMenu.Data
{
    public class MealsDbContext : DbContext
    {
        public MealsDbContext(DbContextOptions<MealsDbContext> options) : base(options) { }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<MainDish> MainDishes { get; set; }
        public DbSet<Soup> Soups { get; set; }
        public DbSet<Dessert> Desserts { get; set; }
        public DbSet<Salad> Salads { get; set; }
        public DbSet<Starter> Starters { get; set; }
        public DbSet<Other> Others { get; set; }
        public DbSet<MenuTemplate> MenuTemplates { get; set; }  

    }
}
