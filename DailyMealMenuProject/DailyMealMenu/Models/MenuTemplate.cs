using System;
using System.ComponentModel.DataAnnotations;

namespace DailyMealMenu.Models
{
    // Kaydedilmiş menü şablonu (tam 4 öğe tutar)
    public class MenuTemplate
    {
        public int Id { get; set; }

        [Required]
        public string ItemsJson { get; set; } = "[]"; // [{kategori, name, catalogId?}]

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // (Opsiyonel) istersen Title veya KaynakTarih gibi alanlar ekleyebilirsin
        public string? Title { get; set; }
    }

    // JSON içine yazacağımız sade item
    public class TemplateItem
    {
        public string kategori { get; set; } = ""; // soups | mainDishes | desserts | salads | starters | others
        public string name { get; set; } = "";
        public int? catalogId { get; set; } // eşleşirse doldururuz
    }
}
