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
        public string OwnerRating { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public bool CanApproveOrReject => Status == "Requested";

        public bool CanMarkReturned => Status == "Approved" || Status == "Out for Rent";

        public bool CanComplete => Status == "Returned";

        public string TimeRemaining
        {
            get
            {
                var now = DateTime.Today;

                if (Status == "Requested")
                    return "Waiting for approval";

                if (Status == "Rejected")
                    return "Rejected";

                if (Status == "Completed")
                    return "Completed";

                if (EndDate.Date < now)
                    return "Overdue";

                var daysLeft = (EndDate.Date - now).Days;

                if (daysLeft == 0)
                    return "Due today";

                return $"{daysLeft} day(s) remaining";
            }
        }

    }



}