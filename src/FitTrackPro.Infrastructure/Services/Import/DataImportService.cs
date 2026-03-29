using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Features.DataImport.Models;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitTrackPro.Infrastructure.Services.Import;

public class DataImportService : IDataImportService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(
        IApplicationDbContext context,
        ILogger<DataImportService> logger)
    {
        _context = context;
        _logger = logger;
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

    public async Task<byte[]> GetFoodImportTemplateAsync(CancellationToken cancellationToken)
    {
        var template = new List<FoodCsvImportRecord>
        {
            new FoodCsvImportRecord
            {
                Name = "Grilled Chicken Breast",
                NameVi = "Ức Gà Nướng",
                Category = "Protein",
                ServingSize = 100,
                ServingUnit = "g",
                Calories = 165,
                Protein = 31,
                Carbs = 0,
                Fat = 3.6m,
                Fiber = 0,
                Sugar = 0
            },
            new FoodCsvImportRecord
            {
                Name = "Brown Rice (Cooked)",
                Category = "Carb",
                ServingSize = 100,
                ServingUnit = "g",
                Calories = 110,
                Protein = 2.6m,
                Carbs = 23,
                Fat = 0.9m
            }
        };

        return await Task.FromResult(GenerateCsv(template));
    }

    public async Task<ImportResult> ImportFoodsFromCsvAsync(Guid userId, Stream csvStream, CancellationToken cancellationToken)
    {
        var result = new ImportResult();

        try
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            });

            var records = csv.GetRecords<FoodCsvImportRecord>().ToList();
            result.TotalRecords = records.Count;

            foreach (var record in records)
            {
                try
                {
                    // Validate record
                    if (string.IsNullOrWhiteSpace(record.Name))
                    {
                        result.Errors.Add($"Row {result.ProcessedRecords + 1}: Food name is required");
                        result.FailedRecords++;
                        result.ProcessedRecords++;
                        continue;
                    }

                    if (record.Calories < 0 || record.ServingSize <= 0)
                    {
                        result.Errors.Add($"Row {result.ProcessedRecords + 1}: Invalid calories or serving size");
                        result.FailedRecords++;
                        result.ProcessedRecords++;
                        continue;
                    }

                    // Check if food already exists
                    var existingFood = await _context.Foods
                        .FirstOrDefaultAsync(f => 
                            f.Name == record.Name && 
                            f.CreatedByUserId == userId);

                    if (existingFood != null)
                    {
                        result.Warnings.Add($"Row {result.ProcessedRecords + 1}: Food '{record.Name}' already exists, skipped");
                        result.SkippedRecords++;
                        result.ProcessedRecords++;
                        continue;
                    }

                    // Create food
                    var macros = new MacroNutrients(
                        record.Protein,
                        record.Carbs,
                        record.Fat
                    );

                    var food = Food.Create(
                        name: record.Name,
                        nameVi: record.NameVi,
                        category: record.Category ?? "Custom",
                        servingSize: record.ServingSize,
                        servingUnit: record.ServingUnit ?? "g",
                        calories: record.Calories,
                        macros: macros,
                        fiber: record.Fiber,
                        sugar: record.Sugar,
                        isUserCreated: true,
                        createdByUserId: userId
                    );

                    _context.Foods.Add(food);
                    result.SuccessfulRecords++;
                    result.ProcessedRecords++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {result.ProcessedRecords + 1}: {ex.Message}");
                    result.FailedRecords++;
                    result.ProcessedRecords++;
                }
            }

            if (result.SuccessfulRecords > 0)
            {
                await _context.SaveChangesAsync();
            }

            result.IsSuccess = result.SuccessfulRecords > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing foods from CSV");
            result.Errors.Add($"General error: {ex.Message}");
            result.IsSuccess = false;
        }

        return result;
    }
}