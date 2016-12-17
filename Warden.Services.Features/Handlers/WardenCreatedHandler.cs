using System.Threading.Tasks;
using Warden.Common.Events;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Services.Organizations.Shared.Events;

namespace Warden.Services.Features.Handlers
{
    public class WardenCreatedHandler : IEventHandler<WardenCreated>
    {
        private readonly IUserFeaturesManager _userFeaturesManager;

        public WardenCreatedHandler(IUserFeaturesManager userFeaturesManager)
        {
            _userFeaturesManager = userFeaturesManager;
        }

        public async Task HandleAsync(WardenCreated @event)
        {
            await _userFeaturesManager.IncreaseFeatureUsageAsync(@event.UserId, FeatureType.AddWarden);
        }
    }
}