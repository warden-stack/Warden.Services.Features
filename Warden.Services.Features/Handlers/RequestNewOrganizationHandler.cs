using System.Threading.Tasks;
using RawRabbit;
using Warden.Common.Commands;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Services.Features.Shared.Events;
using Warden.Services.Organizations.Shared.Commands;

namespace Warden.Services.Features.Handlers
{
    public class RequestNewOrganizationHandler : ICommandHandler<RequestNewOrganization>
    {
        private readonly IBusClient _bus;
        private readonly IUserFeaturesManager _userFeaturesManager;

        public RequestNewOrganizationHandler(IBusClient bus, IUserFeaturesManager userFeaturesManager)
        {
            _bus = bus;
            _userFeaturesManager = userFeaturesManager;
        }

        public async Task HandleAsync(RequestNewOrganization command)
        {
            var featureStatus = await _userFeaturesManager
                .GetFeatureStatusAsync(command.UserId, FeatureType.AddOrganization);
            if (featureStatus != FeatureStatus.Available)
            {
                await _bus.PublishAsync(new FeatureRejected(command.Request.Id,
                    command.UserId, FeatureType.AddOrganization.ToString(),
                    "error","Organization limit reached."));

                return;
            }

            await _bus.PublishAsync(new CreateOrganization
            {
                OrganizationId = command.OrganizationId,
                UserId = command.UserId,
                Name = command.Name,
                Description = command.Description,
                Request = command.Request
            });
        }
    }
}