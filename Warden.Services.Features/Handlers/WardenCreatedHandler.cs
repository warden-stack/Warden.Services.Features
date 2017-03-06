using System.Threading.Tasks;
using Warden.Messages.Events;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Messages.Events.Organizations;

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