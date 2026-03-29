using CsvHelper;
using CsvHelper.Configuration;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Features.DataExport.Models;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Repositories;
using FitTrackPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

public class DataExportService : IDataExportService
{
    private readonly ApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;

    public DataExportService(ApplicationDbContext context, IMealLogRepository mealLogRepository)
    {
        _context = context;
        _mealLogRepository = mealLogRepository;
    }

    private byte[] GenerateCsv<T>(List<T> records)
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        });

        csvWriter.WriteRecords(records);
        streamWriter.Flush();
        return memoryStream.ToArray();
    }

    public async Task<byte[]> ExportMealLogsToCsvAsync(Guid userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.MinValue;
        var end = endDate ?? DateTime.MaxValue;

        var data = await _mealLogRepository.GetByUserIdAndDateRangeAsync(userId, start, end);

        var records = data.Select(m => new MealLogCsvRecord
        {
            Date = m.LoggedAt.ToString("yyyy-MM-dd"),
            Time = m.LoggedAt.ToString("HH:mm"),
            MealType = m.MealType.ToString(),
            FoodName = m.FoodSnapshot.FoodName,
            FoodNameVi = m.FoodSnapshot.FoodNameVi,
            ServingSize = m.FoodSnapshot.ServingSize,
            ServingUnit = m.FoodSnapshot.ServingUnit,
            ServingMultiplier = m.FoodSnapshot.ServingMultiplier,
            Calories = m.FoodSnapshot.TotalCalories,
            Protein = (decimal)m.FoodSnapshot.TotalProtein,
            Carbs = (decimal)m.FoodSnapshot.TotalCarbs,
            Fat = (decimal)m.FoodSnapshot.TotalFat,
            Notes = m.Notes
        }).ToList();

        return GenerateCsv(records);
    }

    public async Task<byte[]> ExportProgressToCsvAsync(Guid userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var query = _context.ProgressEntries.Where(p => p.UserId == userId);
        
        if (startDate.HasValue) query = query.Where(p => p.RecordedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(p => p.RecordedAt <= endDate.Value);

        var data = await query.OrderBy(p => p.RecordedAt).ToListAsync();

        var records = data.Select(p => new ProgressCsvRecord
        {
            Date = p.RecordedAt.ToString("yyyy-MM-dd"),
            Weight = p.Weight,
            BodyFatPercentage = p.BodyFatPercentage,
            Chest = p.Chest,
            Waist = p.Waist,
            Hips = p.Hips,
            Arms = p.Arms,
            Legs = p.Legs,
            Notes = p.Notes
        }).ToList();

        return GenerateCsv(records);
    }

    public async Task<byte[]> ExportNutritionReportAsync(
        Guid userId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken)
    {
        var mealLogs = await _mealLogRepository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);

        // Group by date
        var dailyRecords = mealLogs
            .GroupBy(m => m.LoggedAt.Date)
            .Select(g => new DailyNutritionCsvRecord
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalCalories = g.Sum(m => m.FoodSnapshot.TotalCalories),
                TotalProtein = g.Sum(m => (decimal)m.FoodSnapshot.TotalProtein),
                TotalCarbs = g.Sum(m => (decimal)m.FoodSnapshot.TotalCarbs),
                TotalFat = g.Sum(m => (decimal)m.FoodSnapshot.TotalFat),
                MealCount = g.Count(),
                BreakfastCalories = g.Where(m => m.MealType == MealType.Breakfast)
                    .Sum(m => m.FoodSnapshot.TotalCalories),
                LunchCalories = g.Where(m => m.MealType == MealType.Lunch)
                    .Sum(m => m.FoodSnapshot.TotalCalories),
                DinnerCalories = g.Where(m => m.MealType == MealType.Dinner)
                    .Sum(m => m.FoodSnapshot.TotalCalories),
                SnackCalories = g.Where(m => m.MealType == MealType.Snack)
                    .Sum(m => m.FoodSnapshot.TotalCalories)
            })
            .OrderBy(r => r.Date)
            .ToList();

        return GenerateCsv(dailyRecords);
    }

    public async Task<byte[]> ExportFullDataToCsvAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        // 1. User Info Section
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            streamWriter.WriteLine("=== USER PROFILE ===");
            csv.WriteField("Name"); csv.WriteField($"{user.FirstName} {user.LastName}"); csv.NextRecord();
            csv.WriteField("Email"); csv.WriteField(user.Email); csv.NextRecord();
            streamWriter.WriteLine();
        }

        // 2. Meal Logs Section
        streamWriter.WriteLine("=== MEAL LOGS ===");
        var mealLogs = await _mealLogRepository.GetByUserIdAndDateRangeAsync(userId, DateTime.MinValue, DateTime.MaxValue);
        var mealRecords = mealLogs.Select(m => new MealLogCsvRecord
        {
            Date = m.LoggedAt.ToString("yyyy-MM-dd"),
            Time = m.LoggedAt.ToString("HH:mm"),
            MealType = m.MealType.ToString(),
            FoodName = m.FoodSnapshot.FoodName,
            FoodNameVi = m.FoodSnapshot.FoodNameVi,
            ServingSize = m.FoodSnapshot.ServingSize,
            ServingUnit = m.FoodSnapshot.ServingUnit,
            ServingMultiplier = m.FoodSnapshot.ServingMultiplier,
            Calories = m.FoodSnapshot.TotalCalories,
            Protein = (decimal)m.FoodSnapshot.TotalProtein,
            Carbs = (decimal)m.FoodSnapshot.TotalCarbs,
            Fat = (decimal)m.FoodSnapshot.TotalFat,
            Notes = m.Notes
        });
        
        csv.WriteRecords(mealRecords);
        streamWriter.WriteLine();

        // 3. Progress Section
        streamWriter.WriteLine("=== PROGRESS HISTORY ===");
        var progress = await _context.ProgressEntries.Where(u => u.UserId == userId).ToListAsync();
        var progressRecords = progress.Select(p => new ProgressCsvRecord 
        {
            Date = p.RecordedAt.ToString("yyyy-MM-dd"),
            Weight = p.Weight,
            BodyFatPercentage = p.BodyFatPercentage,
            Chest = p.Chest,
            Waist = p.Waist,
            Hips = p.Hips,
            Arms = p.Arms,
            Legs = p.Legs,
            Notes = p.Notes
        });
        
        csv.WriteRecords(progressRecords);

        streamWriter.Flush();
        return memoryStream.ToArray();
    }
}