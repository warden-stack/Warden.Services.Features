using System.Threading.Tasks;
using RawRabbit;
using Warden.Common.Commands;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Services.Features.Shared.Events;
using Warden.Services.Users.Shared.Commands;

namespace Warden.Services.Features.Handlers
{
    public class RequestNewApiKeyHandler : ICommandHandler<RequestNewApiKey>
    {
        private readonly IBusClient _bus;
        private readonly IUserFeaturesManager _userFeaturesManager;

        public RequestNewApiKeyHandler(IBusClient bus, IUserFeaturesManager userFeaturesManager)
        {
            _bus = bus;
            _userFeaturesManager = userFeaturesManager;
        }

        public async Task HandleAsync(RequestNewApiKey command)
        {
            var featureAvailable = await _userFeaturesManager
                .IsFeatureIfAvailableAsync(command.UserId, FeatureType.AddApiKey);
            if (!featureAvailable)
            {
                await _bus.PublishAsync(new FeatureRejected(command.Request.Id,
                    command.UserId, FeatureType.AddApiKey.ToString(),
                    "error","API key limit reached."));

                return;
            }

            await _bus.PublishAsync(new CreateApiKey
            {
                UserId = command.UserId,
                Name = command.Name,
                Request = command.Request
            });
        }
    }
}