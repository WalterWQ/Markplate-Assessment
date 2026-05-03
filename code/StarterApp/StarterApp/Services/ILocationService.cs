using System;
using System.Collections.Generic;
using System.Text;

namespace StarterApp.Services
{
    public interface ILocationService
    {
        Task<Location?> GetCurrentLocationAsync();
    }
}
