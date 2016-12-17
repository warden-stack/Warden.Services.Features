using System;

namespace Warden.Services.Features.Shared.Dto
{
    public class UserPaymentPlanDto
    {
        public Guid PlanId { get; set; }
        public string Name { get; set; }
        public decimal MonthlyPrice { get; set; }
    }
}