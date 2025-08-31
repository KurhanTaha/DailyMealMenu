namespace DailyMealMenu.Models  // ← bunu proje adına göre düzenle
{
    public class YemekModel
    {
        public string Name { get; set; }
        public string Ingredients { get; set; }
        public int Kcal { get; set; }
        public string Type { get; set; }
    }
}
