using System;
using System.Collections.Generic;
using System.Text;
using StarterApp.Database.Models;

public class ModelTests
{
    [Fact]
    public void Item_ShouldStoreValuesCorrectly()
    {
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            DailyRate = 10,
            Latitude = 55.9533,
            Longitude = -3.1883
        };

        Assert.Equal("Drill", item.Title);
        Assert.Equal(10, item.DailyRate);
    }

    [Fact]
    public void Rental_ShouldSetStatus()
    {
        var rental = new Rental
        {
            Status = "Requested"
        };

        Assert.Equal("Requested", rental.Status);
    }

    [Fact]
    public void User_ShouldStoreEmail()
    {
        var user = new User
        {
            Email = "test@test.com"
        };

        Assert.Equal("test@test.com", user.Email);
    }
}