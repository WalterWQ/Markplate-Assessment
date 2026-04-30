using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private Item? item;

    public ItemDetailViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "Item Details";
    }

    partial void OnItemIdChanged(int value)
    {
        _ = LoadItemAsync();
    }

    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (ItemId <= 0)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            Item = await _apiService.GetAsync<Item>($"items/{ItemId}");
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRentalsAsync()
    {
        if (Item == null)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            await Shell.Current.GoToAsync($"{nameof(Views.RentalsPage)}?itemId={Item.Id}");
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

}