using System;
using System.Collections.Generic;
using System.Text;
using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class RentalsPage : ContentPage
{
    public RentalsPage(RentalsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RentalsViewModel viewModel)
        {
            await viewModel.LoadRentalsCommand.ExecuteAsync(null);
        }
    }
}
