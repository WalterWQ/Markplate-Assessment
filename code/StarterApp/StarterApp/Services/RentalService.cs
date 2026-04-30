using System;
using System.Collections.Generic;
using System.Text;
using StarterApp.Database.Models;


namespace StarterApp.Services
{
    public class RentalService : IRentalService
    {
        private readonly IApiService _apiService;
        public RentalService(IApiService apiService) 
        {
            _apiService = apiService;
        }

        public async Task<Rental?> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }

            var request = new
            {
                itemId,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            };

            return await _apiService.PostAsync<object, Rental>("rentals", request);
        }
        public async Task<List<Rental>> GetIncomingRentalsAsync()
        {
            var response = await _apiService.GetAsync<RentalsResponse>("rentals/incoming");
            return response?.Rentals ?? new List<Rental>();
        }

        public async Task<List<Rental>> GetOutgoingRentalsAsync()
        {
            var response = await _apiService.GetAsync<RentalsResponse>("rentals/outgoing");
            return response?.Rentals ?? new List<Rental>();
        }

        public async Task UpdateStatusAsync(int rentalId, string status)
        {
            var request = new { Status = status };

            await _apiService.PatchAsync<object, RentalStatusResponse>($"/rentals/{rentalId}/status", request);
        }

        private class RentalsResponse
        {
            public List<Rental> Rentals { get; set; } = new();

        }

        private class RentalStatusResponse
        {
            public int Id { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime UpdatedAt { get; set; }
        }



    }
}
