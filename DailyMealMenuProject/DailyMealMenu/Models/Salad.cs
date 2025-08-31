using System.ComponentModel.DataAnnotations.Schema;

namespace DailyMealMenu.Models
{
    public class Salad
    {
        public int Id { get; set; }
        public int? MealId { get; set; }

        [ForeignKey("MealId")]
        public string Name { get; set; }
        public string Ingredients { get; set; }
        public string? ImageUrl { get; set; }
        public int Kcal { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public Meal? Meal { get; set; }
    }
}
