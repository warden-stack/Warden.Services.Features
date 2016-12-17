using System;
using Warden.Common.Events;

namespace Warden.Services.Features.Shared.Events
{
    public class UserPaymentPlanCreated : IAuthenticatedEvent
    {
        public Guid RequestId { get; }
        public string UserId { get; }
        public Guid PlanId { get; }
        public string Name { get; }
        public decimal MonthlyPrice { get; }

        protected UserPaymentPlanCreated()
        {
        }

        public UserPaymentPlanCreated(Guid requestId, string userId, 
            Guid planId, string name, decimal monthlyPrice)
        {
            RequestId = requestId;
            UserId = userId;
            PlanId = planId;
            Name = name;
            MonthlyPrice = monthlyPrice;
        }
    }
}