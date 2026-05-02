using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string editTitle = "";

    [ObservableProperty]
    private string editDescription = "";

    [ObservableProperty]
    private decimal editDailyRate;

    [ObservableProperty]
    private bool editIsAvailable;

    public ItemDetailViewModel(IApiService apiService, IAuthenticationService authService)
    {
        _apiService = apiService;
        _authService = authService;
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
            OnPropertyChanged(nameof(IsOwner));
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

            var route = $"{nameof(Views.RentalsPage)}" + $"?itemId={Item.Id}" + $"&itemTitle={Uri.EscapeDataString(Item.Title)}" + $"&dailyRate={Item.DailyRate}";

            await Shell.Current.GoToAsync(route);
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
    private void StartEditing()
    {
        if (Item == null)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            EditTitle = Item.Title;
            EditDescription = Item.Description ?? "";
            EditDailyRate = Item.DailyRate;
            EditIsAvailable = Item.IsAvailable;
            IsEditing = true;
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
    private async Task SaveItemAsync()
    {
        if (Item == null)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            if (string.IsNullOrWhiteSpace(EditTitle) || EditTitle.Trim().Length < 5)
            {
                SetError("Title must be at least 5 characters.");
                return;
            }

            if (EditDailyRate <= 0 || EditDailyRate > 1000)
            {
                SetError("Daily rate must be between £0.01 and £1000.");
                return;
            }

            var request = new
            {
                title = EditTitle,
                description = EditDescription,
                dailyRate = EditDailyRate,
                isAvailable = EditIsAvailable
            };

            await _apiService.PutAsync<object, Item>($"items/{Item.Id}", request);

            IsEditing = false;
            await LoadItemAsync();


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

    public bool IsOwner => Item != null && _authService.CurrentUser != null && Item.OwnerId == _authService.CurrentUser.Id;

}