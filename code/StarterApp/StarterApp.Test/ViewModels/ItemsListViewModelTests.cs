using System;
using System.Collections.Generic;
using System.Text;
using StarterApp.Database.Data.Repositories;
using StarterApp.Test.Fixtures;

public class ItemsListViewModelTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ItemsListViewModelTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private class MockLocationService
    {
        private readonly (double Latitude, double Longitude) _mockLocation;

        public MockLocationService(double lat, double lon)
        {
            _mockLocation = (lat, lon);
        }

        public Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
        {
            return Task.FromResult(_mockLocation);
        }
    }

    [Fact]
    public async Task LoadNearbyItems_UsesMockCurrentLocation()
    {
        // Arrange
        var mockLocation = new MockLocationService(55.9533, -3.1883);
        var repository = new ItemRepository(_fixture.Context);
        var radiusKm = 5.0;

        // Act
        var location = await mockLocation.GetCurrentLocationAsync();

        var items = await repository.GetNearbyAsync(
            location.Latitude,
            location.Longitude,
            radiusKm);

        // Assert
        Assert.NotEmpty(items);
    }
}