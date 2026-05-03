using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    private readonly NearbyItemsViewModel _viewModel;

    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    private void OnLocationSelected(object sender, EventArgs e)
    {
        if (sender is not Picker picker)
            return;

        switch (picker.SelectedIndex)
        {
            case 0:
                _viewModel.SetLocation(55.9533, -3.1883);
                break;

            case 1:
                _viewModel.SetLocation(55.9336, -3.2138);
                break;

            case 2:
                _viewModel.SetLocation(55.8642, -4.2517);
                break;
        }
    }
}