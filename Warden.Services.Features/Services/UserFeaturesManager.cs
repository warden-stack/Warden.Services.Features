using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Warden.Common.Types;
using Warden.Services.Features.Domain;
using Warden.Services.Features.Repositories;
using Warden.Services.Features.Settings;

namespace Warden.Services.Features.Services
{
    public class UserFeaturesManager : IUserFeaturesManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); 
        private readonly IWardenChecksService _wardenChecksService;
        private readonly IUserRepository _userRepository;
        private readonly IUserPaymentPlanRepository _userPaymentPlanRepository;
        private readonly PaymentPlanSettings _paymentPlanSettings;

        public UserFeaturesManager(IWardenChecksService wardenChecksService,
            IUserRepository userRepository,
            IUserPaymentPlanRepository userPaymentPlanRepository,
            PaymentPlanSettings paymentPlanSettings)
        {
            _wardenChecksService = wardenChecksService;
            _userRepository = userRepository;
            _userPaymentPlanRepository = userPaymentPlanRepository;
            _paymentPlanSettings = paymentPlanSettings;
        }

        public async Task<FeatureStatus> GetFeatureStatusAsync(string userId, FeatureType feature)
        {
            if (!_paymentPlanSettings.Enabled)
                return FeatureStatus.Available;
            if (feature == FeatureType.AddWardenCheck)
                return await _wardenChecksService.GetFeatureStatusAsync(userId);

            var user = await _userRepository.GetAsync(userId);
            if (user.HasNoValue)
                return FeatureStatus.Unavailable;
            if (!user.Value.PaymentPlanId.HasValue)
                return FeatureStatus.Unavailable;
            var paymentPlan = await _userPaymentPlanRepository.GetAsync(user.Value.PaymentPlanId.Value);
            if (paymentPlan.HasNoValue)
                return FeatureStatus.Unavailable;
            var monthlySubscription = paymentPlan.Value.GetMonthlySubscription(DateTime.UtcNow);
            if (monthlySubscription == null)
                return FeatureStatus.Unavailable;
            if (!monthlySubscription.CanUseFeature(feature))
                return FeatureStatus.Unavailable;

            return FeatureStatus.Unavailable;
        }

        public async Task<Maybe<FeatureLimit>> GetFeatureLimitAsync(string userId, FeatureType feature)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user.HasNoValue || !user.Value.PaymentPlanId.HasValue)
                return null;

            var plan = await _userPaymentPlanRepository.GetAsync(user.Value.PaymentPlanId.Value);
            var monthlySubscription = plan.Value.GetMonthlySubscription(DateTime.UtcNow);
            var foundFeature = monthlySubscription.FeatureUsages.FirstOrDefault(x => x.Feature == feature);
            if (foundFeature == null)
                return null;

            return new FeatureLimit
            {
                Usage = foundFeature.Usage,
                AvailableTo = monthlySubscription.To,
                HasMonthlyLimit = true,
                Limit = foundFeature.Limit
            };
        }

        public async Task IncreaseFeatureUsageAsync(string userId, FeatureType feature)
        {
            if (!_paymentPlanSettings.Enabled)
                return;

            if (feature == FeatureType.AddWardenCheck)
            {
                var usage = await _wardenChecksService.IncreaseUsageAsync(userId);
                if (usage % _paymentPlanSettings.FlushWardenChecksLimit != 0)
                    return;
            }

            var user = await _userRepository.GetAsync(userId);
            if (user.HasNoValue)
                throw new ArgumentException($"User {userId} has not been found.");
            if (!user.Value.PaymentPlanId.HasValue)
                throw new InvalidOperationException($"User {userId} has no payment plan assigned.");
            var paymentPlan = await _userPaymentPlanRepository.GetAsync(user.Value.PaymentPlanId.Value);
            if (paymentPlan.HasNoValue)
                throw new InvalidOperationException($"User {userId} payment plan has not been found.");
            var monthlySubscription = paymentPlan.Value.GetMonthlySubscription(DateTime.UtcNow);
            if (monthlySubscription == null)
                throw new InvalidOperationException($"User {userId} monthly subscription has not been found.");
            if (!monthlySubscription.CanUseFeature(feature))
                throw new InvalidOperationException($"Feature {feature} has reached its limit.");

            monthlySubscription.IncreaseFeatureUsage(feature);
            Logger.Debug($"Increasing and saving feature usage: '{feature}' for user: '{userId}'.");
            await _userPaymentPlanRepository.UpdateAsync(paymentPlan.Value);
        }
    }
}