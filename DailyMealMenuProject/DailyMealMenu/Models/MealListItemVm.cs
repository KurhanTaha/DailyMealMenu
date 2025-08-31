// Models/ViewModels/MealListItemVm.cs (ya da mevcut dosyan neresi ise)
using System;
using System.Collections.Generic;

namespace DailyMealMenu.Models
{
    public class MealListItemVm
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        // İsim listeleri
        public List<string> Soups { get; set; } = new();
        public List<string> MainDishes { get; set; } = new();
        public List<string> Desserts { get; set; } = new();
        public List<string> Salads { get; set; } = new();
        public List<string> Starters { get; set; } = new();
        public List<string> Others { get; set; } = new();

        public int MealId { get; set; }
        public int SoupsCount { get; set; }
        public int MainDishesCount { get; set; }
        public int DessertsCount { get; set; }
        public int SaladsCount { get; set; }
        public int StartersCount { get; set; }
        public int OthersCount { get; set; }

        public int TotalCount =>
       SoupsCount + MainDishesCount + DessertsCount + SaladsCount + StartersCount + OthersCount;
    }
}

