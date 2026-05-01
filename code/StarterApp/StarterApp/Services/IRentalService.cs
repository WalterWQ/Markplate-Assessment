using System;
using System.Collections.Generic;
using System.Text;
using StarterApp.Database.Models;

namespace StarterApp.Services
{
    public interface IRentalService
    {
        Task<Rental?> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);
        Task<List<Rental>> GetIncomingRentalsAsync();
        Task<List<Rental>> GetOutgoingRentalsAsync();
        Task UpdateStatusAsync(int rentalId, string status);
    }
}
