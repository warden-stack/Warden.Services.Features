using System;

namespace Warden.Services.Features.Domain
{
    public class FeatureLimit
    {
        public DateTime AvailableTo { get; set ;}
        public int Limit { get; set; }
        public bool HasMonthlyLimit { get; set; }
    }
}