using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;
using static Android.Util.EventLogTags;

namespace StarterApp.ViewModels;

public partial class CreateItemViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private decimal dailyRate;
    [ObservableProperty] private CategoryDto? selectedCategory;
    [ObservableProperty] private List<CategoryDto> categories = new();
    [ObservableProperty] private double latitude;
    [ObservableProperty] private double longitude;

    public CreateItemViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "Create Item";
    }

    public void SetLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int ItemCount { get; set; }
    }

    private class CategoriesResponse
    {
        public List<CategoryDto> Categories { get; set; } = new();
    }

    [RelayCommand]
    private async Task CreateItemAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            if (!ValidateForm())
            {
                return;
            }

            var newItem = new
            {
                title = Title,
                description = Description,
                dailyRate = DailyRate,
                categoryId = SelectedCategory?.Id ?? 0,
                latitude = Latitude,
                longitude = Longitude
            };

            await _apiService.PostAsync<object, Item>("items", newItem);

            await Shell.Current.GoToAsync(".."); // go back
        }
        catch (Exception ex)
        {
            SetError($"Failed to create item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<CategoriesResponse>("categories");
            Categories = response?.Categories ?? new List<CategoryDto>();
            SelectedCategory = Categories.FirstOrDefault();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            SetError("Title is required.");
            return false;
        }

        if (Title.Trim().Length < 5)
        {
            SetError("Title must be at least 5 characters long.");
            return false;
        }

        if (Title.Trim().Length > 100)
        {
            SetError("Title must be less than 100 characters long.");
            return false;
        }

        if (Description?.Length > 1000)
        {
            SetError("Description must be less than 1000 characters long.");
            return false;
        }

        if (DailyRate <= 0)
        {
            SetError("Daily rate must be greater than 0.");
            return false;
        }

        if (DailyRate > 1000)
        {
            SetError("Daily rate must be £1000 or less.");
            return false;
        }

        if (SelectedCategory == null)
        {
            SetError("Please select a category.");
            return false;
        }

        if (Latitude < -90 || Latitude > 90)
        {
            SetError("Latitude must be between -90 and 90.");
            return false;
        }

        if (Longitude < -180 || Longitude > 180)
        {
            SetError("Longitude must be between -180 and 180.");
            return false;
        }

        return true;
    }
}