using System.Threading.Tasks;
using RawRabbit;
using Warden.Common.Commands;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Services.Features.Shared.Events;
using Warden.Services.Organizations.Shared.Commands;

namespace Warden.Services.Features.Handlers
{
    public class RequestNewWardenHandler : ICommandHandler<RequestNewWarden>
    {
        private readonly IBusClient _bus;
        private readonly IUserFeaturesManager _userFeaturesManager;

        public RequestNewWardenHandler(IBusClient bus, 
            IUserFeaturesManager userFeaturesManager)
        {
            _bus = bus;
            _userFeaturesManager = userFeaturesManager;
        }

        public async Task HandleAsync(RequestNewWarden command)
        {
            var featureStatus = await _userFeaturesManager
                .GetFeatureStatusAsync(command.UserId, FeatureType.AddWarden);
            if (featureStatus != FeatureStatus.Available)
            {
                await _bus.PublishAsync(new FeatureRejected(command.Request.Id,
                    command.UserId, FeatureType.AddWarden.ToString(),
                    "error", "Warden limit reached."));

                return;
            }

            await _bus.PublishAsync(new CreateWarden
            {
                UserId = command.UserId,
                Name = command.Name,
                WardenId = command.WardenId,
                OrganizationId = command.OrganizationId,
                Enabled = command.Enabled,
                Request = command.Request
            });
        }
    }
}