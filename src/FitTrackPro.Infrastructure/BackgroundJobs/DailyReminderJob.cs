namespace FitTrackPro.Infrastructure.BackgroundJobs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Domain.Repositories;
using FitTrackPro.Domain.Entities;

public class DailyReminderJob
{
    private readonly ApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<DailyReminderJob> _logger;

    public DailyReminderJob(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<DailyReminderJob> logger,
        IMealLogRepository mealLogRepository)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _mealLogRepository = mealLogRepository;
    }

    public async Task SendDailyRemindersAsync()
    {
        _logger.LogInformation("Starting daily reminder job");

        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var candidates = await _context.Users
                .Where(u => u.EmailConfirmed)
                .Include(u => u.CurrentGoal)
                .ToListAsync();

            // Find users who haven't logged anything today
            var inactiveUsers = new List<User>();
            foreach (var user in candidates)
            {
                var logsToday = await _mealLogRepository.GetByUserIdAndDateRangeAsync(user.Id, today, tomorrow);
                
                if (logsToday.Count == 0)
                {
                    inactiveUsers.Add(user);
                }
            }

            _logger.LogInformation("Found {Count} inactive users", inactiveUsers.Count);

            foreach (var user in inactiveUsers)
            {
                try
                {
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        _logger.LogWarning("User {UserId} has no email address", user.Id);
                        continue;
                    }

                    var goalDetails = user.CurrentGoal != null
                        ? $"Target: {user.CurrentGoal.TargetWeight}kg ({user.CurrentGoal.WeightGoal})"
                        : "No active goal set";

                    await _emailService.SendGoalReminderAsync(
                        user.Email,
                        user.FirstName,
                        goalDetails);

                    _logger.LogInformation("Sent reminder to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder to {Email}", user.Email);
                }

                await Task.Delay(1000);
            }

            _logger.LogInformation("Daily reminder job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily reminder job failed");
            throw;
        }
    }
}
