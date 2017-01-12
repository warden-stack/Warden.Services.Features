using System.Threading.Tasks;
using RawRabbit;
using Warden.Common.Commands;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Services;
using Warden.Services.Features.Shared.Events;
using Warden.Services.WardenChecks.Shared.Commands;

namespace Warden.Services.Features.Handlers
{
    public class RequestProcessWardenCheckResultHandler : ICommandHandler<RequestProcessWardenCheckResult>
    {
        private readonly IBusClient _bus;
        private readonly IUserFeaturesManager _userFeaturesManager;

        public RequestProcessWardenCheckResultHandler(IBusClient bus, IUserFeaturesManager userFeaturesManager)
        {
            _bus = bus;
            _userFeaturesManager = userFeaturesManager;
        }

        public async Task HandleAsync(RequestProcessWardenCheckResult command)
        {
            var featureAvailable = await _userFeaturesManager
                .IsFeatureAvailableAsync(command.UserId, FeatureType.AddWardenCheck);
            if (!featureAvailable)
            {
                await _bus.PublishAsync(new FeatureRejected(command.Request.Id,
                    command.UserId, FeatureType.AddWardenCheck.ToString(),
                    "error","Warden check limit reached."));

                return;
            }

            await _bus.PublishAsync(new ProcessWardenCheckResult
            {
                UserId = command.UserId,
                OrganizationId = command.OrganizationId,
                WardenId = command.WardenId,
                Check = command.Check,
                Request = command.Request
            });
        }
    }
}