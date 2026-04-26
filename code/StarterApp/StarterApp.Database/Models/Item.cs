using System;
using System.Collections.Generic;
using System.Text;

namespace StarterApp.Database.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DailyRate { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsAvailable { get; set; }
        public double? AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
