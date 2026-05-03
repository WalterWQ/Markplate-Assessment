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

        public Task<Item?> GetByIdAsync(int id)
        {
            return Task.FromResult(_context.Items.FirstOrDefault(i => i.Id == id));
        }

        public Task<List<Item>> GetNearbyAsync(double lat, double lon, double radiusKm)
        {
            var nearbyItems = _context.Items
                .ToList()
                .Where(i => CalculateDistance(lat, lon, i.Latitude, i.Longitude) <= radiusKm)
                .ToList();

            return Task.FromResult(nearbyItems);
        }

        public Task<Item> CreateAsync(Item item)
        {
            _context.Items.Add(item);
            _context.SaveChanges();
            return Task.FromResult(item);
        }

        public Task UpdateAsync(Item item)
        {
            _context.Items.Update(item);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

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