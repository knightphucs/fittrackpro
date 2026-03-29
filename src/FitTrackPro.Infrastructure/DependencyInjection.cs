namespace FitTrackPro.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Infrastructure.Services.Authentication;
using FitTrackPro.Infrastructure.Services.Caching;
using FitTrackPro.Infrastructure.Services.Email;
using FitTrackPro.Infrastructure.Services.FileStorage;
using FitTrackPro.Infrastructure.Services.Search;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FitTrackPro.Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Infrastructure.Persistence.Seed;
using FitTrackPro.Infrastructure.Services.Export;
using FitTrackPro.Infrastructure.MachineLearning;
using FitTrackPro.Infrastructure.Services.Common;
using FitTrackPro.Infrastructure.Services.Import;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using FitTrackPro.Infrastructure.Persistence.Repositories.Mongo;
using FitTrackPro.Domain.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment? env = null)
    {
        var elasticUri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
        var defaultIndex = configuration["Elasticsearch:DefaultIndex"] ?? "foods_index";

        // Database
        // In Testing environment, let test project configure DbContext (e.g., Sqlite) to avoid provider conflicts.
        if (env is null || !string.Equals(env.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // In Testing environment, tests will register Identity services explicitly.
        if (env is null || !string.Equals(env.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        }

        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch (BsonSerializationException)
        {
            // Serializer already registered
        }

        services.AddSingleton<IMongoClient>(sp =>
        {
            var mongoConnectionString = configuration.GetConnectionString("MongoDbConnection") ?? "mongodb://localhost:27017";
            var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "FitTrackPro_NoSql";
            return new MongoClient(mongoConnectionString);
        });

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "FitTrackPro_NoSql";
            return client.GetDatabase(databaseName);
        });

        services.AddScoped<IMealLogRepository, MongoMealLogRepository>();
        services.AddScoped<IWorkoutRepository, MongoWorkoutRepository>();
        services.AddScoped<IPersonalRecordRepository, MongoPersonalRecordRepository>();

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "FitTrackPro_";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        /// Email Settings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Cloudinary Settings
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

        // Services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Database Seeder
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<FoodSeeder>();
        services.AddScoped<ExerciseSeeder>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Elasticsearch
        services.AddSingleton(sp =>
        {
            var settings = new ElasticsearchClientSettings(new Uri(elasticUri))
                .DefaultIndex(defaultIndex)
                .PrettyJson()
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);

            return new ElasticsearchClient(settings);
        });
        services.AddScoped<ISearchService, ElasticsearchService>();

        services.AddTransient<IEmailService, EmailService>();
        
        // File Storage - Choose based on configuration
        var storageProvider = configuration["FileStorage:Provider"] ?? "Local";

        switch (storageProvider.ToLower())
        {
            case "azure":
                services.AddScoped<IFileStorageService, AzureBlobStorageService>();
                break;
                
            case "cloudinary":
                services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
                break;
                
            default:
                services.AddScoped<IFileStorageService, LocalFileStorageService>();
                break;
        }

        // Data Export Service
        services.AddScoped<IDataExportService, DataExportService>();

        // Data Import Service
        services.AddScoped<IDataImportService, DataImportService>();

        // PDF Export Service
        services.AddScoped<IPdfExportService, PdfExportService>();

        // ML Prediction Service
        services.AddScoped<IGoalPredictionService, GoalPredictionService>();

        // Background Jobs
        services.AddScoped<BackgroundJobs.WeeklyReportJob>();
        services.AddScoped<BackgroundJobs.DailyReminderJob>();

        return services;
    }
}
