using StarterApp.Database.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace StarterApp.Services
{
    public interface IApiService
    {
        // --- AUTH ---
        event EventHandler<bool>? AuthenticationStateChanged;

        bool IsAuthenticated { get; }
        User? CurrentUser { get; }
        List<string> CurrentUserRoles { get; }

        Task<AuthenticationResult> LoginAsync(string email, string password);
        Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password);
        Task LogoutAsync();

        Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data);

        Task<string?> GetValidAccessTokenAsync();
        Task<bool> IsAuthenticatedAsync();

        bool HasRole(string roleName);
        bool HasAnyRole(params string[] roleNames);
        bool HasAllRoles(params string[] roleNames);

        // --- GENERIC API CALLS ---
        Task<T?> GetAsync<T>(string endpoint);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);
        Task<bool> DeleteAsync(string endpoint);
    }
}
