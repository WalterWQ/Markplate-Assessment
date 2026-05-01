using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
[QueryProperty(nameof(ItemTitle), "itemTitle")]
[QueryProperty(nameof(DailyRate), "dailyRate")]
public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private string itemTitle = "";

    [ObservableProperty]
    private decimal dailyRate;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(2);

    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<Rental> outgoingRentals = new();

    public RentalsViewModel(IRentalService rentalService)
    {
        _rentalService = rentalService;
        Title = "Rentals";
    }

    public decimal EstimatedTotal => EndDate > StartDate ? DailyRate * (EndDate - StartDate).Days : 0;

    partial void OnStartDateChanged(DateTime value) => OnPropertyChanged(nameof(EstimatedTotal));

    partial void OnEndDateChanged(DateTime value) => OnPropertyChanged(nameof(EstimatedTotal));

    partial void OnDailyRateChanged(decimal value) => OnPropertyChanged(nameof(EstimatedTotal));

    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        if (ItemId <= 0)
        {
            SetError("No item selected.");
            return;
        }

        if (EndDate <= StartDate)
        {
            SetError("End date must be after start date.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            await _rentalService.RequestRentalAsync(ItemId, StartDate, EndDate);

            await Shell.Current.DisplayAlert("Success", "Rental request submitted.", "OK");

            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to request rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var incoming = await _rentalService.GetIncomingRentalsAsync();
            var outgoing = await _rentalService.GetOutgoingRentalsAsync();

            IncomingRentals.Clear();
            OutgoingRentals.Clear();

            foreach (var rental in incoming)
                IncomingRentals.Add(rental);

            foreach (var rental in outgoing)
                OutgoingRentals.Add(rental);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(int rentalId) =>
        await UpdateRentalStatusAsync(rentalId, "Approved");

    [RelayCommand]
    private async Task RejectRentalAsync(int rentalId) =>
        await UpdateRentalStatusAsync(rentalId, "Rejected");

    [RelayCommand]
    private async Task CompleteRentalAsync(int rentalId) =>
        await UpdateRentalStatusAsync(rentalId, "Completed");

    [RelayCommand]
    private async Task MarkReturnedAsync(int rentalId) =>
        await UpdateRentalStatusAsync(rentalId, "Returned");

    private async Task UpdateRentalStatusAsync(int rentalId, string status)
    {
        try
        {
            ClearError();

            await _rentalService.UpdateStatusAsync(rentalId, status);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to update rental: {ex.Message}");
        }
    }
}