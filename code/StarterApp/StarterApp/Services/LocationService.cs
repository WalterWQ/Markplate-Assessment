using System;
using System.Collections.Generic;
using System.Text;

namespace StarterApp.Services;

/// <summary>
/// Location service that handles retrieving the user's current location
/// </summary>
public class LocationService : ILocationService
{
    /// <summary>
    /// Asynchronously retrieves the device's current geographic location if location permissions are granted.
    /// </summary>
    public async Task<Location?> GetCurrentLocationAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        /// Is the users location permission granted?
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            return null;
        }

        var request = new GeolocationRequest(
            GeolocationAccuracy.Medium,
            TimeSpan.FromSeconds(10));

        return await Geolocation.Default.GetLocationAsync(request);
    }
}
