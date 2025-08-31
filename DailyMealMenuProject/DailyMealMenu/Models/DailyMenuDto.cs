namespace DailyMealMenu.Models
{
    public class MealItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Kcal { get; set; }
    }

    public class DailyMenuDto
    {
        public string Date { get; set; } = ""; // "yyyy-MM-dd"
        public List<MealItemDto> soups { get; set; } = new();
        public List<MealItemDto> mainDishes { get; set; } = new();
        public List<MealItemDto> desserts { get; set; } = new();
        public List<MealItemDto> salads { get; set; } = new();
        public List<MealItemDto> starters { get; set; } = new();
        public List<MealItemDto> others { get; set; } = new();
    }
}
