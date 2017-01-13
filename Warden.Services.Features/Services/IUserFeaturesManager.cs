using System.Threading.Tasks;
using Warden.Common.Types;
using Warden.Services.Features.Domain;

namespace Warden.Services.Features.Services
{
    public interface IUserFeaturesManager
    {
        Task<FeatureStatus> GetFeatureStatusAsync(string userId, FeatureType feature);
        Task<Maybe<FeatureLimit>> GetFeatureLimitAsync(string userId, FeatureType feature);
        Task IncreaseFeatureUsageAsync(string userId, FeatureType feature);
    }
}