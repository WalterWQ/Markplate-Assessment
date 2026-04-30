using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;

    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<Rental> outgoingRentals = new();

    public RentalsViewModel(IRentalService rentalService)
    {
        _rentalService = rentalService;
        Title = "Rentals";
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
    private async Task ApproveRentalAsync(int rentalId)
    {
        await UpdateRentalStatusAsync(rentalId, "Approved");
    }

    [RelayCommand]
    private async Task RejectRentalAsync(int rentalId)
    {
        await UpdateRentalStatusAsync(rentalId, "Rejected");
    }

    [RelayCommand]
    private async Task CompleteRentalAsync(int rentalId)
    {
        await UpdateRentalStatusAsync(rentalId, "Completed");
    }

    [RelayCommand]
    private async Task MarkReturnedAsync(int rentalId)
    {
        await UpdateRentalStatusAsync(rentalId, "Returned");
    }

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