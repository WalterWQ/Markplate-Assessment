using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using System.Collections.ObjectModel;

namespace StarterApp.ViewModels;

public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    public ItemsListViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "Items";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var response = await _apiService.GetAsync<ItemsResponse>("items");

            Items.Clear();

            foreach (var item in response?.Items ?? new List<Item>())
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
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

    private class ItemsResponse
    {
        public List<Item> Items { get; set; } = new();
    }
}