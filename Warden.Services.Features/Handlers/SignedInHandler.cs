using System.Threading.Tasks;
using RawRabbit;
using Warden.Common.Events;
using Warden.Services.Features.Repositories;
using Warden.Services.Features.Services;
using Warden.Services.Users.Shared.Events;

namespace Warden.Services.Features.Handlers
{
    public class SignedInHandler : IEventHandler<SignedIn>
    {
        private readonly IBusClient _bus;
        private readonly IUserRepository _userRepository;
        private readonly IUserPaymentPlanService _userPaymentPlanService;
        private readonly IWardenChecksService _wardenChecksCounter;

        public SignedInHandler(IBusClient bus,
            IUserRepository userRepository,
            IUserPaymentPlanService userPaymentPlanService,
            IWardenChecksService wardenChecksCounter)
        {
            _bus = bus;
            _userRepository = userRepository;
            _userPaymentPlanService = userPaymentPlanService;
            _wardenChecksCounter = wardenChecksCounter;
        }

        public async Task HandleAsync(SignedIn @event)
        {
            var user = await _userRepository.GetAsync(@event.UserId);
            if(user.HasValue)
                return;
        }
    }
}