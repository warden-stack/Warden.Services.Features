using System;
using System.Threading.Tasks;
using NLog;
using Warden.Common.Caching;
using Warden.Common.Types;
using Warden.Services.Features.Domain;

namespace Warden.Services.Features.Services
{
    public class WardenChecksService : IWardenChecksService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); 
        private readonly ICache _cache;

        public WardenChecksService(ICache cache)
        {
            _cache = cache;
        }

        public async Task InitializeAsync(string userId, int usage, int limit, DateTime availableTo)
        {
            Logger.Debug($"Checking if WardenChecks feature is initialized for user: '{userId}'.");
            var featureUsage = await _cache.GetAsync<WardenChecksUsage>(GetKey(userId));
            if (featureUsage.HasValue)
                return;

            featureUsage = new WardenChecksUsage
            {
                Usage = usage,
                Limit = limit,
                AvailableTo = availableTo.Ticks
            };
            await _cache.AddAsync(GetKey(userId), featureUsage.Value, TimeSpan.FromTicks(availableTo.Ticks));
            Logger.Debug($"Initialized WardenChecks feature usage for user: '{userId}' " + 
                         $"with usage of: {usage}/{limit} and it will be available to: {availableTo}.");
        }

        public async Task<FeatureStatus> GetFeatureStatusAsync(string userId)
        {
            var usage = await _cache.GetAsync<WardenChecksUsage>(GetKey(userId));

            return GetFeatureStatus(userId, usage);
        }

        public async Task<int> IncreaseUsageAsync(string userId)
        {
            Logger.Debug($"Increasing WardenChecks feature usage for user: '{userId}'.");
            var key = GetKey(userId);
            var usage = await _cache.GetAsync<WardenChecksUsage>(key);
            var featureStatus = GetFeatureStatus(userId, usage);
            if (featureStatus != FeatureStatus.Available)
                return 0;

            usage.Value.Usage++;
            await _cache.AddAsync(key, usage.Value);
            Logger.Debug($"WardenChecks feature usage was increased for user: '{userId}' to: {usage.Value.Usage}.");

            return usage.Value.Usage;
        }

        private FeatureStatus GetFeatureStatus(string userId, Maybe<WardenChecksUsage> usage)
        {
            if (usage.HasNoValue)
            {
                Logger.Debug($"WardenChecks feature for user: '{userId}' does not exist.");

                return FeatureStatus.NotFound;
            }
            if (usage.Value.Usage >= usage.Value.Limit || DateTime.UtcNow.Ticks > usage.Value.AvailableTo) 
            {
                Logger.Debug($"Limit of {usage.Value.Limit} WardenChecks feature usage for user: '{userId}' was reached.");   

                return FeatureStatus.Unavailable;
            }

            return FeatureStatus.Available;           
        }

        public async Task<int> GetUsageAsync(string userId)
        {
            var usage = await _cache.GetAsync<WardenChecksUsage>(GetKey(userId));

            return usage.HasValue ? usage.Value.Usage : 0;
        }

        public async Task SetUsageAsync(string userId, int usage, DateTime availableTo)
        {
            Logger.Debug($"Setting WardenChecks feature usage for user: '{userId}' to: {usage}.");
            var key = GetKey(userId);
            var featureUsage = await _cache.GetAsync<WardenChecksUsage>(key);
            if (featureUsage.HasNoValue)
                return;

            featureUsage.Value.AvailableTo = availableTo.Ticks;
            featureUsage.Value.Usage = usage;
            await _cache.AddAsync(key, featureUsage.Value);
            Logger.Debug($"WardenChecks feature usage for user: '{userId}' was set to: {usage}.");
        }

        private static string GetKey(string userId) => $"{userId}:checks";

        private class WardenChecksUsage
        {
            public long AvailableTo { get; set; }
            public int Limit { get; set; }
            public int Usage { get; set; }
        }
    }
}