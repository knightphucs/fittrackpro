using Hangfire;

namespace FitTrackPro.Infrastructure.BackgroundJobs;

public static class JobScheduler
{
    public static void ScheduleJobs()
    {
        // Send weekly reports every Monday at 8 AM
        RecurringJob.AddOrUpdate<WeeklyReportJob>(
            "weekly-reports",
            job => job.SendWeeklyReportsAsync(),
            Cron.Weekly(DayOfWeek.Monday, 8),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });

        // Daily reminder for inactive users (at 6 PM)
        RecurringJob.AddOrUpdate<DailyReminderJob>(
            "daily-reminders",
            job => job.SendDailyRemindersAsync(),
            Cron.Daily(18),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });
    }
}
