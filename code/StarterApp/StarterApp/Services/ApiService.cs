using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiService : IApiService, IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private User? _currentUser;
    private readonly List<string> _currentUserRoles = new();

    private const string accessTokenKey = "token";
    private const string refreshTokenKey = "token_refresh";
    private const string expiryKey = "token_expiry";

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/token", new { email, password });

            var rawBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"LOGIN STATUS: {(int)response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"LOGIN BODY: {rawBody}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new AuthenticationResult(false, error?.Message ?? "Login failed");
            }

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (token == null || string.IsNullOrWhiteSpace(token.Token))
            {
                return new AuthenticationResult(false, "Login failed: Invalid token response");
            }

            await SaveTokenAsync(token.Token, token.ExpiresAt);

            await PrepareAuthenticatedRequest();
            var meResponse = await _httpClient.GetAsync("users/me");

            if (!meResponse.IsSuccessStatusCode)
            {
                return new AuthenticationResult(false, "Login failed: Unable to retrieve user profile");
            }

            var profile = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();

            if (profile == null)
            {
                return new AuthenticationResult(false, "Login failed: Invalid user profile response");
            }

            _currentUser = new User
            {
                Id = profile!.Id,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                CreatedAt = profile.CreatedAt,
                IsActive = true
            };

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            var rawBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Register response: {(int)response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Register Body: {rawBody}");

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    return new AuthenticationResult(false, error?.Message ?? $"Registration failed: {rawBody}");
                }
                catch
                {
                    return new AuthenticationResult(false, $"Registration failed: {rawBody}");
                }
                
            }

            return new AuthenticationResult(true, "Registration successful. Please log in.");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    private async Task SaveTokenAsync(string accessToken, DateTime expiresAt)
    {
        await SecureStorage.Default.SetAsync(accessTokenKey, accessToken);

        await SecureStorage.Default.SetAsync(expiryKey, expiresAt.ToString("O"));  
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        var refreshToken = await GetStoredRefreshTokenAsync();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await ClearStoredTokenAsync();
            return false;
        }
        
        var response = await _httpClient.PostAsJsonAsync("auth/refresh", new { refreshToken });

        if (!response.IsSuccessStatusCode)
        {
            await ClearStoredTokenAsync(); 
            return false;
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (token == null || string.IsNullOrWhiteSpace(token.Token))
        {
            await ClearStoredTokenAsync();
            return false;
        }

        await SaveTokenAsync(token.Token, token.ExpiresAt);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);

        return true;
    }

    public async Task<string?> GetValidAccessTokenAsync()
    {
        var token = await GetStoredAccessTokenAsync();
        var expiry = await GetStoredTokenExpiryAsync();

        if (string.IsNullOrWhiteSpace(token) || expiry == null)
        {
            return null;
        }

        if(DateTime.UtcNow >= expiry.Value.AddMinutes(-1))
        {
            await ClearStoredTokenAsync(); 
            return null;
        }

        return token;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await GetValidAccessTokenAsync();

        System.Diagnostics.Debug.WriteLine($"Restoring session with token: {(string.IsNullOrWhiteSpace(token) ? "null or empty" : token)}");

        if (string.IsNullOrWhiteSpace(token))
        {
            System.Diagnostics.Debug.WriteLine("No valid token found during session restore.");
            return false;
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await PrepareAuthenticatedRequest();
        var meResponse = await _httpClient.GetAsync("users/me");

        System.Diagnostics.Debug.WriteLine($"Me endpoint response during session restore: {(int)meResponse.StatusCode}");

        if (!meResponse.IsSuccessStatusCode)
        {
            await LogoutAsync();
            System.Diagnostics.Debug.WriteLine("Failed to retrieve user profile during session restore. Logging out.");
            return false;
        }

        var profile = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();

        if (profile == null)
        {
            await LogoutAsync();
            System.Diagnostics.Debug.WriteLine("Invalid user profile response during session restore. Logging out.");
            return false;
        }

        _currentUser = new User
        {
            Id = profile.Id,
            Email = profile.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            CreatedAt = profile.CreatedAt,
            IsActive = true
        };

        AuthenticationStateChanged?.Invoke(this, true);
        System.Diagnostics.Debug.WriteLine("Session restored successfully.");
        return true;
    }

    private Task<string?> GetStoredAccessTokenAsync() =>
        SecureStorage.Default.GetAsync(accessTokenKey);

    private Task<string?> GetStoredRefreshTokenAsync() =>
        SecureStorage.Default.GetAsync(refreshTokenKey);

    public Task<bool> IsAuthenticatedAsync() => Task.FromResult(IsAuthenticated);

    private async Task<DateTime?> GetStoredTokenExpiryAsync()
    {
        var raw = await SecureStorage.Default.GetAsync(expiryKey);

        if (DateTime.TryParse(raw, out var expiry))
        {
            return expiry;
        }

        return null;
    }

    private Task ClearStoredTokenAsync()
    {
        SecureStorage.Default.Remove(accessTokenKey);
        SecureStorage.Default.Remove(refreshTokenKey);
        SecureStorage.Default.Remove(expiryKey);
        return Task.CompletedTask;
    }

    private async Task PrepareAuthenticatedRequest()
    {
        var token = await GetValidAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = null;
        await ClearStoredTokenAsync();
        AuthenticationStateChanged?.Invoke(this, false);
    }

    public bool HasRole(string roleName) =>
        _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public bool HasAnyRole(params string[] roleNames) =>
        roleNames.Any(HasRole);

    public bool HasAllRoles(params string[] roleNames) =>
        roleNames.All(HasRole);

    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        // Not supported by the shared API
        return Task.FromResult(false);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await PrepareAuthenticatedRequest();
        return await _httpClient.GetFromJsonAsync<T>(endpoint);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await PrepareAuthenticatedRequest();

        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await PrepareAuthenticatedRequest();

        var response = await _httpClient.PutAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        await PrepareAuthenticatedRequest();

        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    // --- API Json Sync ---

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await PrepareAuthenticatedRequest();
 
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
        {
            Content = JsonContent.Create(data)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>();

    }

    // --- API response DTOs ---

    private record TokenResponse(string Token, DateTime ExpiresAt, int UserId);

    private record UserProfileResponse(
        int Id, string Email, string FirstName, string LastName, DateTime CreatedAt);

    private record ApiErrorResponse(string Error, string Message);
}