using System;
using System.Collections.Generic;
using System.Text;
using StarterApp.Database.Models;


namespace StarterApp.Services
{
   
    public class RentalService : IRentalService
    {
        private readonly IApiService _apiService;
        /// <summary>
        /// Initializes a new instance of the RentalService class using the API service.
        /// </summary>
        /// <param name="apiService">The API service used to perform backend operations required by the rental service</param>
        public RentalService(IApiService apiService) 
        {
            _apiService = apiService;
        }
        /// <summary>
        /// Requests a rental for the specified item within the  date range.
        /// </summary>
        /// <param name="itemId">The unique identifier of the item to be rented.</param>
        /// <param name="startDate">The start date of the rental period.</param>
        /// <param name="endDate">The end date of the rental period.</param>
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

        /// <summary>
        /// retrieves a list of incoming rental requests.
        /// </summary>
        /// <returns>A list of rental objects representing incoming rentals. Returns an empty list if there are no incoming rentals.</returns>
        public async Task<List<Rental>> GetIncomingRentalsAsync()
        {
            var response = await _apiService.GetAsync<RentalsResponse>("rentals/incoming");
            return response?.Rentals ?? new List<Rental>();
        }

        /// <summary>
        /// retrieves the list of rentals initiated by the current user.
        /// </summary>
        public async Task<List<Rental>> GetOutgoingRentalsAsync()
        {
            var response = await _apiService.GetAsync<RentalsResponse>("rentals/outgoing");

            return response?.Rentals ?? new List<Rental>();
        }

        /// <summary>
        /// updates the status of a rental with the specified identifier.
        /// </summary>
        /// <param name="rentalId">The unique identifier of the rental to update.</param>
        /// <param name="status">The new status to assign to the rental. Cannot be null or empty.</param>

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
