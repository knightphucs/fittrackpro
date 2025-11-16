namespace FitTrackPro.Infrastructure.Services.Email;

using Microsoft.Extensions.Logging;
using FitTrackPro.Application.Common.Interfaces;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        // TODO: Implement actual email sending (SendGrid, SMTP, etc.)
        _logger.LogInformation("Sending welcome email to {Email}", email);
        await Task.CompletedTask;
    }

    public async Task SendGoalReminderAsync(string email, string firstName, string goalDetails)
    {
        _logger.LogInformation("Sending goal reminder to {Email}", email);
        await Task.CompletedTask;
    }

    public async Task SendWeeklySummaryAsync(string email, string firstName, object summaryData)
    {
        _logger.LogInformation("Sending weekly summary to {Email}", email);
        await Task.CompletedTask;
    }
}