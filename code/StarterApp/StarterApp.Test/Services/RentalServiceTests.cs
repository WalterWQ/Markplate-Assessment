using System;
using System.Collections.Generic;
using System.Text;
using StarterApp.Database.Models;

namespace StarterApp.Test.Services;

public class RentalServiceTests
{
    private class MockRentalApi
    {
        public List<Rental> Rentals { get; } = new();

        public Task<Rental> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
                throw new InvalidOperationException("End date must be after start date.");

            var rental = new Rental
            {
                Id = Rentals.Count + 1,
                ItemId = itemId,
                ItemTitle = "Test Item",
                BorrowerId = 1,
                BorrowerName = "Test User",
                OwnerId = 2,
                OwnerName = "Owner User",
                StartDate = startDate,
                EndDate = endDate,
                Status = "Requested",
                TotalPrice = 10
            };

            Rentals.Add(rental);
            return Task.FromResult(rental);
        }

        public Task<List<Rental>> GetOutgoingRentalsAsync()
        {
            return Task.FromResult(Rentals);
        }

        public Task UpdateStatusAsync(int rentalId, string status)
        {
            var rental = Rentals.FirstOrDefault(r => r.Id == rentalId);

            if (rental == null)
                throw new InvalidOperationException("Rental not found.");

            rental.Status = status;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CreateRentalAsync_WithValidDates_ShouldCreateRental()
    {
        // Arrange
        var service = new MockRentalApi();

        // Act
        var rental = await service.CreateRentalAsync(
            1,
            DateTime.Today.AddDays(1),
            DateTime.Today.AddDays(2));

        // Assert
        Assert.NotNull(rental);
        Assert.Equal(1, rental.ItemId);
        Assert.Equal("Requested", rental.Status);
    }

    [Fact]
    public async Task CreateRentalAsync_WithInvalidDates_ShouldThrowException()
    {
        // Arrange
        var service = new MockRentalApi();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRentalAsync(
                1,
                DateTime.Today.AddDays(2),
                DateTime.Today.AddDays(1)));
    }

    [Fact]
    public async Task GetOutgoingRentalsAsync_ShouldReturnCreatedRental()
    {
        // Arrange
        var service = new MockRentalApi();

        await service.CreateRentalAsync(
            1,
            DateTime.Today.AddDays(1),
            DateTime.Today.AddDays(2));

        // Act
        var rentals = await service.GetOutgoingRentalsAsync();

        // Assert
        Assert.Single(rentals);
        Assert.Equal("Test Item", rentals[0].ItemTitle);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldChangeRentalStatus()
    {
        // Arrange
        var service = new MockRentalApi();

        var rental = await service.CreateRentalAsync(
            1,
            DateTime.Today.AddDays(1),
            DateTime.Today.AddDays(2));

        // Act
        await service.UpdateStatusAsync(rental.Id, "Approved");

        // Assert
        Assert.Equal("Approved", rental.Status);
    }

    [Theory]
    [InlineData("Requested", true)]
    [InlineData("Approved", false)]
    [InlineData("Rejected", false)]
    [InlineData("Returned", false)]
    [InlineData("Completed", false)]
    public void CanApproveOrReject_ShouldOnlyBeTrueForRequested(string status, bool expected)
    {
        // Arrange
        var rental = new Rental
        {
            Status = status,
            EndDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = rental.CanApproveOrReject;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Approved", true)]
    [InlineData("Out for Rent", true)]
    [InlineData("Requested", false)]
    [InlineData("Returned", false)]
    [InlineData("Completed", false)]
    public void CanMarkReturned_ShouldOnlyBeTrueForApprovedOrOutForRent(string status, bool expected)
    {
        // Arrange
        var rental = new Rental
        {
            Status = status,
            EndDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = rental.CanMarkReturned;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Returned", true)]
    [InlineData("Requested", false)]
    [InlineData("Approved", false)]
    [InlineData("Completed", false)]
    public void CanComplete_ShouldOnlyBeTrueForReturned(string status, bool expected)
    {
        // Arrange
        var rental = new Rental
        {
            Status = status,
            EndDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = rental.CanComplete;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TimeRemaining_WhenRequested_ShouldReturnWaitingForApproval()
    {
        // Arrange
        var rental = new Rental
        {
            Status = "Requested",
            EndDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = rental.TimeRemaining;

        // Assert
        Assert.Equal("Waiting for approval", result);
    }

    [Fact]
    public void TimeRemaining_WhenCompleted_ShouldReturnCompleted()
    {
        // Arrange
        var rental = new Rental
        {
            Status = "Completed",
            EndDate = DateTime.Today.AddDays(2)
        };

        // Act
        var result = rental.TimeRemaining;

        // Assert
        Assert.Equal("Completed", result);
    }

    [Fact]
    public void TimeRemaining_WhenEndDateIsToday_ShouldReturnDueToday()
    {
        // Arrange
        var rental = new Rental
        {
            Status = "Approved",
            EndDate = DateTime.Today
        };

        // Act
        var result = rental.TimeRemaining;

        // Assert
        Assert.Equal("Due today", result);
    }

    [Fact]
    public void TimeRemaining_WhenEndDateIsPast_ShouldReturnOverdue()
    {
        // Arrange
        var rental = new Rental
        {
            Status = "Approved",
            EndDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = rental.TimeRemaining;

        // Assert
        Assert.Equal("Overdue", result);
    }

    [Fact]
    public void TimeRemaining_WhenFutureDate_ShouldReturnDaysRemaining()
    {
        // Arrange
        var rental = new Rental
        {
            Status = "Approved",
            EndDate = DateTime.Today.AddDays(3)
        };

        // Act
        var result = rental.TimeRemaining;

        // Assert
        Assert.Equal("3 day(s) remaining", result);
    }
}