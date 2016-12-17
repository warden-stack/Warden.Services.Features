using System.Threading.Tasks;
using Warden.Common.Events;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Services.Organizations.Shared.Events;

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