using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private double latitude = 55.9533;

    [ObservableProperty]
    private double longitude = -3.1883;

    [ObservableProperty]
    private double radiusMiles = 5;

    public NearbyItemsViewModel(IApiService apiService, ILocationService locationService)
    {
        _apiService = apiService;
        _locationService = locationService;
        Title = "Nearby Items";
    }

    [RelayCommand]
    private async Task UseCurrentLocationAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var location = await _locationService.GetCurrentLocationAsync();

            if (location == null)
            {
                SetError("Could not get your current location.");
                return;
            }

            Latitude = location.Latitude;
            Longitude = location.Longitude;

            await FindNearbyAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to get location: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task FindNearbyAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var radiusKm = RadiusMiles * 1.60934;

            var response = await _apiService.GetAsync<NearbyItemsResponse>(
                $"items/nearby?lat={Latitude}&lon={Longitude}&radius={radiusKm}");

            Items.Clear();

            foreach (var item in response?.Items ?? new List<Item>())
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to find nearby items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenItemAsync(int itemId)
    {
        await Shell.Current.GoToAsync($"{nameof(Views.ItemDetailPage)}?itemId={itemId}");
    }

    public void SetLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    private class NearbyItemsResponse
    {
        public List<Item> Items { get; set; } = new();
    }
}