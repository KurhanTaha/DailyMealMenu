namespace DailyMealMenu.Models
{
    public class Meal
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public ICollection<MainDish> MainDishes { get; set; }
        public ICollection<Soup> Soups { get; set; }
        public ICollection<Dessert> Desserts { get; set; }
        public ICollection<Salad> Salads { get; set; }
        public ICollection<Starter> Starters { get; set; }
        public ICollection<Other> Others { get; set; }
    }
}
