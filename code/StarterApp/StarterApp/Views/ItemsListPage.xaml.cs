using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemsListPage : ContentPage
{
    public ItemsListPage(ItemsListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ItemsListViewModel viewModel)
        {
            await viewModel.LoadItemsCommand.ExecuteAsync(null);
        }
    }
}