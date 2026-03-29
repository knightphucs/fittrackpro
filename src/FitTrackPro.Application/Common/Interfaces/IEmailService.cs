namespace FitTrackPro.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendGoalReminderAsync(string email, string firstName, string goalDetails);
    Task SendWeeklySummaryAsync(string email, string firstName, object summaryData);
    Task SendPasswordResetEmailAsync(string email, string encodedToken);
    Task SendEmailConfirmationAsync(string email, string encodedToken);
}
