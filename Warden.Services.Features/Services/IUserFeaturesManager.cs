﻿using System.Threading.Tasks;
using Warden.Services.Features.Domain;

namespace Warden.Services.Features.Services
{
    public interface IUserFeaturesManager
    {
        Task<bool> IsFeatureAvailableAsync(string userId, FeatureType feature);
        Task IncreaseFeatureUsageAsync(string userId, FeatureType feature);
    }
}