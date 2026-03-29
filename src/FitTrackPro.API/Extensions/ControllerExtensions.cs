using System.Text.Json.Serialization;

namespace FitTrackPro.API.Extensions;

public static class ControllerExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        services.AddEndpointsApiExplorer();
        return services;
    }
}
