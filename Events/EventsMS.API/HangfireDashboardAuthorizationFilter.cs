using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace EventsMS.API
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
