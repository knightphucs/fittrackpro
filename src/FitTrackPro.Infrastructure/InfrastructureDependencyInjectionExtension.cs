namespace FitTrackPro.Infrastructure;
 
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Infrastructure.MachineLearning;
using FitTrackPro.Infrastructure.Services.Export;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
 
public static class InfrastructureDependencyInjectionExtension
{
    public static IServiceCollection AddMachineLearning(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FoodRecognitionOptions>(
            configuration.GetSection(FoodRecognitionOptions.SectionName));
 
        services.AddSingleton<IFoodRecognitionService, FoodRecognitionService>();
 
        services.AddScoped<IModelTrainer, ModelTrainer>();
 
        return services;
    }
}