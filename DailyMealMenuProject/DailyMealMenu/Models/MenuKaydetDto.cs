namespace DailyMealMenu.Models
{
    public class SecilenYemekDto
    {
        public int YemekId { get; set; }
        public string Kategori { get; set; } = string.Empty; // soups, mainDishes, desserts, salads, starters, others
    }

    public class MenuKaydetDto
    {
        public string Tarih { get; set; } = string.Empty; // "YYYY-MM-DD"
        public List<SecilenYemekDto> Yemekler { get; set; } = new();
    }
}
