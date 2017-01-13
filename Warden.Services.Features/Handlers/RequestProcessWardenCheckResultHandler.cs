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
        private readonly IWardenChecksService _wardenChecksService;

        public RequestProcessWardenCheckResultHandler(IBusClient bus, IUserFeaturesManager userFeaturesManager,
            IWardenChecksService wardenChecksService)
        {
            _bus = bus;
            _userFeaturesManager = userFeaturesManager;
            _wardenChecksService = wardenChecksService;
        }

        public async Task HandleAsync(RequestProcessWardenCheckResult command)
        {
            var featureStatus = await _userFeaturesManager
                .GetFeatureStatusAsync(command.UserId, FeatureType.AddWardenCheck);
            if (featureStatus == FeatureStatus.Unavailable)
            {
                await _bus.PublishAsync(new FeatureRejected(command.Request.Id,
                    command.UserId, FeatureType.AddWardenCheck.ToString(),
                    "error","Warden check limit reached."));

                return;
            }
            if (featureStatus == FeatureStatus.NotFound)
            {
                var featureLimit = await _userFeaturesManager.GetFeatureLimitAsync(command.UserId, FeatureType.AddWardenCheck);
                if (featureLimit.HasNoValue)
                    return;

                await _wardenChecksService.InitializeAsync(command.UserId, featureLimit.Value.Limit, featureLimit.Value.AvailableTo);
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