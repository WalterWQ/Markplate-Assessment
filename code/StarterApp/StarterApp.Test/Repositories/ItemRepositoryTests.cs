using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;
using Xunit.Abstractions;

public class ItemRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ItemRepositoryTests(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetNearbyAsync_ShouldReturnItemsWithinRadius()
    {
        var repository = new ItemRepository(_fixture.Context);

        var testLat = 55.9533;
        var testLon = -3.1883;
        var radiusKm = 5.0;

        var items = await repository.GetNearbyAsync(testLat, testLon, radiusKm);

        _output.WriteLine($"Items found: {items.Count}");

        Assert.NotEmpty(items);

        foreach (var item in items)
        {
            var distance = ItemRepository.CalculateDistance(
                testLat,
                testLon,
                item.Latitude,
                item.Longitude);

            _output.WriteLine($"{item.Title} → {distance:F2} km");

            Assert.True(distance <= radiusKm);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSeededItems()
    {
        var repository = new ItemRepository(_fixture.Context);

        var items = await repository.GetAllAsync();

        Assert.NotEmpty(items);
        Assert.True(items.Count >= 2);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCorrectItem()
    {
        var repository = new ItemRepository(_fixture.Context);

        var item = await repository.GetByIdAsync(1);

        Assert.NotNull(item);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddItemToDatabase()
    {
        var repository = new ItemRepository(_fixture.Context);

        var item = new Item
        {
            Title = "Test Item",
            Description = "Test Description",
            DailyRate = 12,
            CategoryId = 1,
            OwnerId = 1,
            Latitude = 55.9533,
            Longitude = -3.1883,
            IsAvailable = true
        };

        var created = await repository.CreateAsync(item);
        var saved = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(saved);
        Assert.Equal("Test Item", saved.Title);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingItem()
    {
        var repository = new ItemRepository(_fixture.Context);
        var item = await repository.GetByIdAsync(1);

        Assert.NotNull(item);

        item.Title = "Updated Drill";

        await repository.UpdateAsync(item);
        var updated = await repository.GetByIdAsync(1);

        Assert.NotNull(updated);
        Assert.Equal("Updated Drill", updated.Title);
    }

    [Fact]
    public async Task Users_ShouldContainSeededUser()
    {
        var user = await _fixture.Context.Users
            .FirstOrDefaultAsync(u => u.Email == "test@example.com");

        Assert.NotNull(user);
        Assert.Equal("Test", user.FirstName);
    }

    [Fact]
    public async Task Roles_ShouldContainSeededRole()
    {
        var role = await _fixture.Context.Roles
            .FirstOrDefaultAsync(r => r.Name == "User");

        Assert.NotNull(role);
        Assert.Equal("Standard user", role.Description);
    }

    [Fact]
    public async Task UserRoles_ShouldLinkUserAndRole()
    {
        var userRole = await _fixture.Context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync();

        Assert.NotNull(userRole);
        Assert.Equal("test@example.com", userRole.User.Email);
        Assert.Equal("User", userRole.Role.Name);
    }

    [Fact]
    public async Task GetNearbyAsync_WithNoItems_ShouldReturnEmpty()
    {
        var emptyContext = new DatabaseFixture().Context;
        var repository = new ItemRepository(emptyContext);

        var items = await repository.GetNearbyAsync(0, 0, 1);

        Assert.Empty(items);
    }

    [Fact]
    public async Task Users_ShouldAllowAddingNewUser()
    {
        var user = new User
        {
            Email = "new@example.com",
            FirstName = "New",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _fixture.Context.Users.Add(user);
        await _fixture.Context.SaveChangesAsync();

        var saved = await _fixture.Context.Users
            .FirstOrDefaultAsync(u => u.Email == "new@example.com");

        Assert.NotNull(saved);
        Assert.Equal("New", saved.FirstName);
    }

    [Fact]
    public void CalculateDistance_ShouldIncreaseWithDistance()
    {
        var d1 = ItemRepository.CalculateDistance(55.9533, -3.1883, 55.9533, -3.1883);
        var d2 = ItemRepository.CalculateDistance(55.9533, -3.1883, 56.0, -3.0);

        Assert.True(d2 > d1);
    }
}