namespace FitTrackPro.API.IntegrationTests.Services.Email;

using FitTrackPro.Application.Common.Interfaces;

public class FakeEmailService : IEmailService
{
    public Task SendWelcomeEmailAsync(string email, string name) => Task.CompletedTask;
    public Task SendGoalReminderAsync(string email, string firstName, string goalDetails) => Task.CompletedTask;
    public Task SendWeeklySummaryAsync(string email, string firstName, object summaryData) => Task.CompletedTask;
}
