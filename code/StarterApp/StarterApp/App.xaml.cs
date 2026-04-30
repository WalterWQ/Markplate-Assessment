using StarterApp.Services;
using StarterApp.ViewModels;

namespace StarterApp;

public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IAuthenticationService _authenticationService;
    public App(IServiceProvider serviceProvider, IAuthenticationService authenticationService)
	{
		_serviceProvider = serviceProvider;
		_authenticationService = authenticationService;
		InitializeComponent();

        Routing.RegisterRoute(nameof(Views.ItemsListPage), typeof(Views.ItemsListPage));
        Routing.RegisterRoute(nameof(Views.ItemDetailPage), typeof(Views.ItemDetailPage));
        Routing.RegisterRoute(nameof(Views.CreateItemPage), typeof(Views.CreateItemPage));
        Routing.RegisterRoute(nameof(Views.RentalsPage), typeof(Views.RentalsPage));
        Routing.RegisterRoute(nameof(Views.MainPage), typeof(Views.MainPage));
		Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
		Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
		Routing.RegisterRoute(nameof(Views.UserListPage), typeof(Views.UserListPage));
		Routing.RegisterRoute(nameof(Views.UserDetailPage), typeof(Views.UserDetailPage));
		Routing.RegisterRoute(nameof(Views.TempPage), typeof(Views.TempPage));
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// var window = base.CreateWindow(activationState);
		// window.Page = new AppShell();

		var shell = _serviceProvider.GetService<AppShell>();
		if (shell == null)
		{
			// Handle the error if AppShell could not be resolved
			throw new InvalidOperationException("AppShell could not be resolved from the service provider.");
		}
		var window = new Window(shell);

		_ = RestoreSessionAsync();

		return window;
	}

    private async Task RestoreSessionAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Attempting to restore session...");

            var restored = await _authenticationService.TryRestoreSessionAsync();

            System.Diagnostics.Debug.WriteLine($"Session restore result: {restored}");

            if (restored)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    System.Diagnostics.Debug.WriteLine("Navigating to MainPage...");
                    await Shell.Current.GoToAsync(nameof(Views.MainPage));
                });
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    System.Diagnostics.Debug.WriteLine("No valid session found, navigating to LoginPage...");
                    await Shell.Current.GoToAsync(nameof(Views.LoginPage));
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"APP: restore failed with exception: {ex.Message}");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Views.LoginPage));
            });
        }
    }
}
