using System;
using System.Threading.Tasks;

namespace Warden.Services.Features.Services
{
    public interface IWardenChecksService
    {
        Task InitializeAsync(string userId, int limit, DateTime availableTo);
        Task<bool> CanUseAsync(string userId);
        Task<int> IncreaseUsageAsync(string userId);
        Task<int> GetUsageAsync(string userId);
        Task SetUsageAsync(string userId, int usage, DateTime availableTo);
    }
}