using System;
using System.Linq;
using System.Threading.Tasks;
using RawRabbit;
using Warden.Common.Events;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Repositories;
using Warden.Services.Features.Services;
using Warden.Services.Features.Shared.Events;
using Warden.Services.Users.Shared.Events;

namespace Warden.Services.Features.Handlers
{
    public class SignedUpHandler : IEventHandler<SignedUp>
    {
        private readonly IBusClient _bus;
        private readonly IUserRepository _userRepository;
        private readonly IUserPaymentPlanService _userPaymentPlanService;
        private readonly IWardenChecksService _wardenChecksCounter;

        public SignedUpHandler(IBusClient bus,
            IUserRepository userRepository,
            IUserPaymentPlanService userPaymentPlanService,
            IWardenChecksService wardenChecksCounter)
        {
            _bus = bus;
            _userRepository = userRepository;
            _userPaymentPlanService = userPaymentPlanService;
            _wardenChecksCounter = wardenChecksCounter;
        }

        public async Task HandleAsync(SignedUp @event)
        {
            var user = await _userRepository.GetAsync(@event.UserId);
            if(user.HasValue)
                return;

            await _userRepository.AddAsync(new User(@event.Email, @event.UserId, @event.Role, @event.State));
            await _userPaymentPlanService.CreateDefaultAsync(@event.UserId);
            var plan = await _userPaymentPlanService.GetCurrentPlanAsync(@event.UserId);
            var addWardenChecksFeature = plan.Value.Features.First(x => x.Type == FeatureType.AddWardenCheck);
            var monthlySubscription = plan.Value.GetMonthlySubscription(DateTime.UtcNow);
            await _wardenChecksCounter.InitializeAsync(@event.UserId, addWardenChecksFeature.Limit, monthlySubscription.To);
            await _bus.PublishAsync(new UserPaymentPlanCreated(Guid.NewGuid(), @event.UserId, plan.Value.Id,
                plan.Value.Name, plan.Value.MonthlyPrice));
        }
    }
}