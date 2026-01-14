using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace BookingMS.API
{
    [ExcludeFromCodeCoverage]
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();          
            return true; 
        }
    }
}
