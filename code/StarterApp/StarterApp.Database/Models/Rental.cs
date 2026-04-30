using System;
using System.Collections.Generic;
using System.Text;

namespace StarterApp.Database.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal RentalPrice { get; set; }
        public DateTime? RequestedTime { get; set; }
        public DateTime? CreatedTime { get; set; }

    }
}
