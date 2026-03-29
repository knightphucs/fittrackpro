using FitTrackPro.Domain.Constants;

namespace FitTrackPro.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole",
                policy => policy.RequireRole(Roles.Administrator));
        });

        return services;
    }
}
