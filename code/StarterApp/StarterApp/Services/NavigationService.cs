namespace StarterApp.Services;
/// <summary>
/// NavigationService provides methods for navigating between pages in the application using Shell navigation.
/// </summary>
public class NavigationService : INavigationService
{
    /// <summary>
    /// Navigates to the specified route.
    /// </summary>
    /// <param name="route">The route/"page" to navigate to.</param>
    public async Task NavigateToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }

    /// <summary>
    /// navigates to the specified route in the navigation    
    /// </summary>
    /// <param name="route">The route to navigate to.</param>
    /// <param name="parameters">A dictionary containing parameter names</param>
    public async Task NavigateToAsync(string route, Dictionary<string, object> parameters)
    {
        await Shell.Current.GoToAsync(route, parameters);
    }

    /// <summary>
    /// Navigates to the previous page in the navigation.
    /// </summary>
    public async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// Navigates to the applications root page, used to reset navigation to the login screen.
    /// </summary>

    public async Task NavigateToRootAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
    
    /// <summary>
    /// Navigates to the root page of the navigation stack removing all intermediate pages.
    /// </summary>
    public async Task PopToRootAsync()
    {
        await Shell.Current.Navigation.PopToRootAsync();
    }
}