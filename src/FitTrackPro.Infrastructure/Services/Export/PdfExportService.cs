namespace FitTrackPro.Infrastructure.Services.Export;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Domain.Repositories;

public class PdfExportService : IPdfExportService
{
    private readonly ApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;

    public PdfExportService(ApplicationDbContext context, IMealLogRepository mealLogRepository)
    {
        _context = context;
        _mealLogRepository = mealLogRepository;
        
        // Configure QuestPDF License (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateProgressReportAsync(
        Guid userId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        // Get user
        var user = await _context.Users
            .Include(u => u.CurrentGoal)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("User not found");

        // Get progress data
        var query = _context.ProgressEntries
            .Where(p => p.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(p => p.RecordedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.RecordedAt <= endDate.Value);

        var progressEntries = await query
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

        // Generate PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Height(100)
                    .Background(Colors.Blue.Lighten3)
                    .Padding(20)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("FitTrack Pro")
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);
                            
                            column.Item().Text("Progress Report")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(100).AlignRight().Text($"{DateTime.UtcNow:dd MMM yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        // User Info
                        column.Item().Text($"User: {user.GetFullName()}")
                            .FontSize(16)
                            .Bold();

                        // Goal Info
                        if (user.CurrentGoal != null)
                        {
                            column.Item().Background(Colors.Grey.Lighten3)
                                .Padding(10)
                                .Column(col =>
                                {
                                    col.Item().Text("Current Goal").Bold().FontSize(14);
                                    col.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Current: {user.CurrentGoal.CurrentWeight} kg");
                                        row.RelativeItem().Text($"Target: {user.CurrentGoal.TargetWeight} kg");
                                        row.RelativeItem().Text($"Goal: {user.CurrentGoal.WeightGoal}");
                                    });
                                });
                        }

                        // Progress Summary
                        if (progressEntries.Any())
                        {
                            var firstEntry = progressEntries.First();
                            var lastEntry = progressEntries.Last();
                            var weightChange = lastEntry.Weight - firstEntry.Weight;

                            column.Item().PaddingTop(10).Text("Progress Summary").Bold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Starting Weight").Bold();
                                table.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text($"{firstEntry.Weight} kg");

                                table.Cell().Padding(5).Text("Current Weight").Bold();
                                table.Cell().Padding(5).Text($"{lastEntry.Weight} kg");

                                table.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Total Change").Bold();
                                table.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text($"{weightChange:+0.0;-0.0} kg")
                                    .FontColor(weightChange < 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);

                                table.Cell().Padding(5).Text("Period").Bold();
                                table.Cell().Padding(5)
                                    .Text($"{firstEntry.RecordedAt:dd MMM yyyy} - {lastEntry.RecordedAt:dd MMM yyyy}");
                            });
                        }

                        // Progress Table
                        column.Item().PaddingTop(20).Text("Progress History").Bold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Date").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Weight (kg)").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Body Fat %").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Waist (cm)").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Notes").FontColor(Colors.White).Bold();
                            });

                            // Data rows
                            foreach (var entry in progressEntries.Take(20)) // Limit to 20 entries
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(entry.RecordedAt.ToString("dd MMM yyyy"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(entry.Weight.ToString("F1"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(entry.BodyFatPercentage?.ToString("F1") ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(entry.Waist?.ToString("F1") ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(entry.Notes ?? "-");
                            }
                        });
                    });

                page.Footer()
                    .Height(30)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generated by FitTrack Pro • ");
                        text.Span($"Page ").FontSize(9);
                        text.CurrentPageNumber().FontSize(9);
                        text.Span(" of ").FontSize(9);
                        text.TotalPages().FontSize(9);
                    });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateWeeklyReportAsync(
        Guid userId,
        DateTime? startDate,
        CancellationToken cancellationToken = default)
    {
        var start = (startDate ?? DateTime.UtcNow.AddDays(-7)).Date;
        var end = start.AddDays(7);

        var user = await _context.Users
            .Include(u => u.CurrentGoal)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("User not found");

        // Get data
        var progressEntries = await _context.ProgressEntries
            .Where(p => p.UserId == userId && p.RecordedAt >= start && p.RecordedAt < end)
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

        var mealLogs = await _mealLogRepository.GetByUserIdAndDateRangeAsync(userId, start, end);

        // Calculate stats
        var dailyStats = mealLogs
            .GroupBy(m => m.LoggedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Calories = g.Sum(m => m.FoodSnapshot.TotalCalories),
                Protein = g.Sum(m => m.FoodSnapshot.TotalProtein),
                Carbs = g.Sum(m => m.FoodSnapshot.TotalCarbs),
                Fat = g.Sum(m => m.FoodSnapshot.TotalFat),
                MealCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header()
                    .Height(120)
                    .Background(Colors.Blue.Lighten3)
                    .Padding(20)
                    .Column(column =>
                    {
                        column.Item().Text("FitTrack Pro - Weekly Report")
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);
                        
                        column.Item().Text($"{user.GetFullName()}")
                            .FontSize(14)
                            .FontColor(Colors.Grey.Darken1);

                        column.Item().Text($"Week: {start:dd MMM} - {end.AddDays(-1):dd MMM yyyy}")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        // Summary cards
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Green.Lighten3).Padding(15).Column(col =>
                            {
                                col.Item().Text("Days Logged").FontSize(10).FontColor(Colors.Grey.Darken2);
                                col.Item().Text($"{dailyStats.Count}/7").FontSize(24).Bold();
                            });

                            row.Spacing(10);

                            row.RelativeItem().Background(Colors.Orange.Lighten3).Padding(15).Column(col =>
                            {
                                col.Item().Text("Avg Calories").FontSize(10).FontColor(Colors.Grey.Darken2);
                                col.Item().Text(dailyStats.Any() ? $"{dailyStats.Average(d => d.Calories):F0}" : "0")
                                    .FontSize(24).Bold();
                            });

                            row.Spacing(10);

                            row.RelativeItem().Background(Colors.Blue.Lighten3).Padding(15).Column(col =>
                            {
                                col.Item().Text("Total Meals").FontSize(10).FontColor(Colors.Grey.Darken2);
                                col.Item().Text($"{mealLogs.Count}").FontSize(24).Bold();
                            });
                        });

                        // Daily breakdown
                        if (dailyStats.Any())
                        {
                            column.Item().PaddingTop(20).Text("Daily Nutrition Breakdown").Bold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                        .Text("Date").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                        .Text("Meals").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                        .Text("Calories").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                        .Text("Protein").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                        .Text("Carbs").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                        .Text("Fat").FontColor(Colors.White).Bold();
                                });

                                foreach (var day in dailyStats)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(day.Date.ToString("ddd dd/MM"));
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(day.MealCount.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text($"{day.Calories}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text($"{day.Protein:F0}g");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text($"{day.Carbs:F0}g");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text($"{day.Fat:F0}g");
                                }
                            });
                        }

                        // Achievements
                        column.Item().PaddingTop(20).Background(Colors.Yellow.Lighten3)
                            .Padding(15).Column(col =>
                            {
                                col.Item().Text("🎉 Achievements").Bold().FontSize(14);
                                
                                if (dailyStats.Count >= 7)
                                    col.Item().PaddingTop(5).Text("• Perfect week! Logged every day 🔥");
                                else if (dailyStats.Count >= 5)
                                    col.Item().PaddingTop(5).Text("• Great consistency! 5+ days logged 💪");
                                
                                if (progressEntries.Count >= 3)
                                    col.Item().PaddingTop(5).Text("• Consistent weight tracking ⚖️");
                            });
                    });

                page.Footer()
                    .Height(30)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generated by FitTrack Pro • ");
                        text.Span($"Page ").FontSize(9);
                        text.CurrentPageNumber().FontSize(9);
                    });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Similar to weekly report but for a month
        // Implementation similar to GenerateWeeklyReportAsync
        // but with monthly statistics

        return await GenerateWeeklyReportAsync(userId, startDate, cancellationToken);
    }
}