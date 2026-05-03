using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Test.Fixtures;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();

        SeedTestData();
    }

    private void SeedTestData()
    {
        var items = new List<Item>
        {
            new Item
            {
                Id = 1,
                Title = "Electric Drill",
                Description = "Cordless drill",
                DailyRate = 5,
                CategoryId = 1,
                OwnerId = 1,
                Latitude = 55.9533,
                Longitude = -3.1883,
                IsAvailable = true
            },
            new Item
            {
                Id = 2,
                Title = "Camping Tent",
                Description = "Four person tent",
                DailyRate = 15,
                CategoryId = 2,
                OwnerId = 1,
                Latitude = 55.9600,
                Longitude = -3.1900,
                IsAvailable = true
            }
        };

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var role = new Role
        {
            Id = 1,
            Name = "User",
            Description = "Standard user"
        };

        var userRole = new UserRole
        {
            Id = 1,
            UserId = 1,
            RoleId = 1
        };

        Context.Users.Add(user);
        Context.Roles.Add(role);
        Context.UserRoles.Add(userRole);
        Context.Items.AddRange(items);

        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}