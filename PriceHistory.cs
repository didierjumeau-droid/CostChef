using System;

namespace CostChef
{
    public class PriceHistory
    {
        public int Id { get; set; }
        public int IngredientId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeDate { get; set; }
        public string ChangedBy { get; set; } = "System";
        public string Reason { get; set; }
        
        // Computed properties
        public decimal PriceChange => NewPrice - OldPrice;
        public decimal PercentChange => OldPrice > 0 ? (PriceChange / OldPrice) * 100 : 0;
        public bool IsPriceIncrease => PriceChange > 0;
    }
}