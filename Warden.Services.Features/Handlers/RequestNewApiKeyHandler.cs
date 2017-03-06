using System.Threading.Tasks;
using RawRabbit;
using Warden.Messages.Commands;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Messages.Events.Features;
using Warden.Messages.Commands.Users;

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
            var featureStatus = await _userFeaturesManager
                .GetFeatureStatusAsync(command.UserId, FeatureType.AddApiKey);
            if (featureStatus != FeatureStatus.Available)
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