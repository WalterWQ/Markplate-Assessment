using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories
{
    public class ItemRepository : IItemRepository
    {
        public Task<List<Item>> GetAllAsync()
        {
            return Task.FromResult(new List<Item>());
        }

        public Task<Item?> GetByIdAsync(int id)
        {
            return Task.FromResult<Item?>(null);
        }

        public Task<List<Item>> GetNearbyAsync(double lat, double lon, double radiusKm)
        {
            return Task.FromResult(new List<Item>());
        }

        public Task<Item> CreateAsync(Item item)
        {
            return Task.FromResult(item);
        }

        public Task UpdateAsync(Item item)
        {
            return Task.CompletedTask;
        }
    }
}