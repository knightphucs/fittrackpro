using Serilog;
using FitTrackPro.Application;
using FitTrackPro.Infrastructure;
using FitTrackPro.API.Middlewares;
using FitTrackPro.API.Extensions;
using FitTrackPro.Infrastructure.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilogLogging(builder.Configuration);

// Add core services
builder.Services.AddApiControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddMachineLearning(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCorsPolicy();
builder.Services.AddHangfireJobs(builder.Configuration);
builder.Services.AddHealthCheckServices(builder.Configuration);
builder.Services.ConfigureFileUpload(builder.Configuration);
builder.WebHost.ConfigureFileUploadLimits(builder.Configuration);


var app = builder.Build();

app.UseSwaggerPipeline(app.Environment);
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseFileStorage(builder.Configuration);

app.UseHangfireDashboardWithAuth(builder.Environment);

app.MapControllers();
app.MapHealthChecks("/health");

await app.MigrateAndSeedDatabaseAsync();

if (!app.Environment.IsEnvironment("Testing"))
{
    JobScheduler.ScheduleJobs();
}

app.Run();

public partial class Program { }
