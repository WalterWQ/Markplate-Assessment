using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }
        public Task<List<Item>> GetAllAsync()
        {
            return Task.FromResult(_context.Items.ToList());
        }


        /// <summary>
        /// retrieves an item with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item to retrieve.</param>
        public Task<Item?> GetByIdAsync(int id)
        {
            return Task.FromResult(_context.Items.FirstOrDefault(i => i.Id == id));
        }

        /// <summary>
        /// retrieves a list of items located within a specified radius of the given coordinates. 
        /// </summary>
        /// <param name="lat">The latitude of the center point for the search.</param>
        /// <param name="lon">The longitude of the center point for the search.</param>
        /// <param name="radiusKm">The search radius, in kilometers. Only items within this distance from the specified coordinates are
        /// returned.</param>
        public Task<List<Item>> GetNearbyAsync(double lat, double lon, double radiusKm)
        {
            var nearbyItems = _context.Items
                .ToList()
                .Where(i => CalculateDistance(lat, lon, i.Latitude, i.Longitude) <= radiusKm)
                .ToList();

            return Task.FromResult(nearbyItems);
        }

        /// <summary>
        /// adds a new item to the data store.
        /// </summary>
        /// <param name="item">The item to add to the data store</param>
        public Task<Item> CreateAsync(Item item)
        {
            _context.Items.Add(item);
            _context.SaveChanges();
            return Task.FromResult(item);
        }

        /// <summary>
        /// updates the specified item in the data store.
        /// </summary>
        /// <param name="item">The item to update</param>
        public Task UpdateAsync(Item item)
        {
            _context.Items.Update(item);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Calculates the distance between the cordinates
        /// </summary>
        /// <param name="lat1">The latitude of the first location</param>
        /// <param name="lon1">The longitude of the first location</param>
        /// <param name="lat2">The latitude of the second location</param>
        /// <param name="lon2">The longitude of the second location</param>
        /// <returns>The distance between the two locations in kilometers  </returns>
        public static double CalculateDistance(double lat1, double lon1, double? lat2, double? lon2)
        {
            if (lat2 == null || lon2 == null)
                return double.MaxValue;

            const double earthRadiusKm = 6371;

            var dLat = DegreesToRadians(lat2.Value - lat1);
            var dLon = DegreesToRadians(lon2.Value - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2.Value)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}