using System.Threading.Tasks;
using Warden.Messages.Events;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Messages.Events.Organizations;

namespace Warden.Services.Features.Handlers
{
    public class OrganizationCreatedHandler : IEventHandler<OrganizationCreated>
    {
        private readonly IUserFeaturesManager _userFeaturesManager;

        public OrganizationCreatedHandler(IUserFeaturesManager userFeaturesManager)
        {
            _userFeaturesManager = userFeaturesManager;
        }

        public async Task HandleAsync(OrganizationCreated @event)
        {
            await _userFeaturesManager.IncreaseFeatureUsageAsync(@event.UserId, FeatureType.AddOrganization);
        }
    }
}