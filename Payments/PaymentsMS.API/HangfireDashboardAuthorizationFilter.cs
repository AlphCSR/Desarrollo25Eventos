using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace PaymentsMS.API
{
    [ExcludeFromCodeCoverage]
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}
