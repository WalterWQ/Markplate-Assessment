using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateItemPage : ContentPage
{
    private readonly CreateItemViewModel _viewModel;

    public CreateItemPage(CreateItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;

        var defaultLocation = new Location(55.9533, -3.1883); // Edinburgh
    }

    private void OnLocationSelected(object sender, EventArgs e)
    {
        if (sender is not Picker picker)
            return;

        switch (picker.SelectedIndex)
        {
            case 0:
                _viewModel.SetLocation(55.9336, -3.2138);
                break;
            case 1:
                _viewModel.SetLocation(55.9533, -3.1883);
                break;
            case 2:
                _viewModel.SetLocation(55.8642, -4.2518);
                break;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCategoriesCommand.ExecuteAsync(null);
    }
}