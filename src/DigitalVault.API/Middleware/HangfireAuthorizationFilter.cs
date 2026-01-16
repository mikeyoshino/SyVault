using Hangfire.Dashboard;

namespace DigitalVault.API.Middleware;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow all users in development (for testing)
        // In production, you should check for authentication/authorization
        return true;
    }
}
