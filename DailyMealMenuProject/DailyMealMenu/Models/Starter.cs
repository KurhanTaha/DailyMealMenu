namespace DailyMealMenu.Models
{
    public class Starter
    {
        public int Id { get; set; }
        public int? MealId { get; set; }

        public string Name { get; set; }
        public string Ingredients { get; set; }
        public string? ImageUrl { get; set; }
        public int Kcal { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public Meal? Meal { get; set; }
    }
}
