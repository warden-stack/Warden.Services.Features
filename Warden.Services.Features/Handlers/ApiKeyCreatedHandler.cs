using System.Threading.Tasks;
using Warden.Messages.Events;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Messages.Events.Users;

namespace Warden.Services.Features.Handlers
{
    public class ApiKeyCreatedHandler : IEventHandler<ApiKeyCreated>
    {
        private readonly IUserFeaturesManager _userFeaturesManager;

        public ApiKeyCreatedHandler(IUserFeaturesManager userFeaturesManager)
        {
            _userFeaturesManager = userFeaturesManager;
        }

        public async Task HandleAsync(ApiKeyCreated @event)
        {
            await _userFeaturesManager.IncreaseFeatureUsageAsync(@event.UserId, FeatureType.AddApiKey);
        }
    }
}