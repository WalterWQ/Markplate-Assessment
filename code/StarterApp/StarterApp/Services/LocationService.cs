using System;
using System.Collections.Generic;
using System.Text;

namespace StarterApp.Services;

public class LocationService : ILocationService
{
    public async Task<Location?> GetCurrentLocationAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

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
