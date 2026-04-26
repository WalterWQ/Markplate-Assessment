using StarterApp.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarterApp.Database.Data.Repositories
{
    public interface IItemRepository
    {
        Task<List<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<List<Item>> GetNearbyAsync(double lat, double lon, double radiusKm);
        Task<Item> CreateAsync(Item item);
        Task UpdateAsync(Item item);
    }
}
