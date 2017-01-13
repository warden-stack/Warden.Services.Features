using System;
using System.Threading.Tasks;
using Warden.Services.Features.Domain;

namespace Warden.Services.Features.Services
{
    public interface IWardenChecksService
    {
        Task InitializeAsync(string userId, int usage, int limit, DateTime availableTo);
        Task<FeatureStatus> GetFeatureStatusAsync(string userId);
        Task<int> IncreaseUsageAsync(string userId);
        Task<int> GetUsageAsync(string userId);
        Task SetUsageAsync(string userId, int usage, DateTime availableTo);
    }
}