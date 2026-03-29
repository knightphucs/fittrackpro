namespace FitTrackPro.Infrastructure.BackgroundJobs;

using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Infrastructure.Services.Email;

public class WeeklyReportJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPdfExportService _pdfService;
    private readonly ILogger<WeeklyReportJob> _logger;

    public WeeklyReportJob(
        ApplicationDbContext context,
        IEmailService emailService,
        IPdfExportService pdfService,
        ILogger<WeeklyReportJob> logger)
    {
        _context = context;
        _emailService = emailService;
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task SendWeeklyReportsAsync()
    {
        _logger.LogInformation("Starting weekly report job");

        try
        {
            // Get all active users
            var users = await _context.Users
                .Where(u => u.EmailConfirmed)
                .ToListAsync();

            _logger.LogInformation("Found {Count} users to send reports", users.Count);

            foreach (var user in users)
            {
                try
                {
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        _logger.LogWarning("User {UserId} has no email address", user.Id);
                        continue;
                    }

                    // Generate PDF report
                    var pdfReport = await _pdfService.GenerateWeeklyReportAsync(
                        user.Id,
                        DateTime.UtcNow.AddDays(-7));

                    // Send email with attachment
                    var emailService = (EmailService)_emailService;
                    await emailService.SendWeeklyReportWithAttachmentAsync(
                        user.Email,
                        user.FirstName,
                        pdfReport);

                    _logger.LogInformation("Sent weekly report to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send report to {Email}", user.Email);
                }

                // Rate limiting - don't spam the email server
                await Task.Delay(1000);
            }

            _logger.LogInformation("Weekly report job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weekly report job failed");
            throw;
        }
    }
}
