namespace FitTrackPro.Infrastructure.MachineLearning;

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Infrastructure.MachineLearning.Models;
using FitTrackPro.Domain.Enums;
using FitTrackPro.Domain.Entities;
using Microsoft.ML.Trainers.FastTree;
using FitTrackPro.Application.Features.Analytics.DTOs;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Repositories;

public class GoalPredictionService : IGoalPredictionService
{
    private readonly MLContext _mlContext;
    private readonly ApplicationDbContext _context;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly ILogger<GoalPredictionService> _logger;
    private TransformerChain<RegressionPredictionTransformer<FastTreeRegressionModelParameters>>? _model;

    public GoalPredictionService(
        ApplicationDbContext context,
        ILogger<GoalPredictionService> logger,
        IMealLogRepository mealLogRepository)
    {
        _mlContext = new MLContext(seed: 0);
        _context = context;
        _logger = logger;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<GoalPredictionResult?> PredictGoalAchievementAsync(Guid userId)
    {
        try
        {
            // Get user data
            var user = await _context.Users
                .Include(u => u.CurrentGoal)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.CurrentGoal == null)
            {
                _logger.LogWarning("User {UserId} has no active goal", userId);
                return null;
            }

            // Get historical data
            var progressEntries = await _context.ProgressEntries
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.RecordedAt)
                .ToListAsync();

            var mealLogsList = await _mealLogRepository
                .GetByUserIdAndDateRangeAsync(userId, DateTime.MinValue, DateTime.MaxValue);
            var mealLogs = mealLogsList.ToList();

            if (progressEntries.Count < 3)
            {
                _logger.LogWarning("Not enough data for prediction. Need at least 3 progress entries");
                return null;
            }

            // Calculate features
            var averageDailyCalories = CalculateAverageDailyCalories(mealLogs);
            var currentWeight = progressEntries.Last().Weight;
            var targetWeight = user.CurrentGoal.TargetWeight;
            var age = user.GetAge() ?? 25;
            var gender = user.Gender == Gender.Male ? 1f : 0f;
            var activityLevel = (float)user.CurrentGoal.ActivityLevel;

            // Train or load model
            if (_model == null)
            {
                _model = TrainModel(progressEntries, mealLogs, user);
            }

            // Make prediction
            var predictionEngine = _mlContext.Model
                .CreatePredictionEngine<WeightPredictionInput, WeightPredictionOutput>(_model);

            // Predict weight in 7, 14, 30, 60, 90 days
            var predictions = new List<(int days, float weight)>();
            
            for (int days = 7; days <= 90; days += 7)
            {
                var input = new WeightPredictionInput
                {
                    DayNumber = days,
                    CurrentWeight = (float)currentWeight,
                    AverageDailyCalories = averageDailyCalories,
                    ActivityLevel = activityLevel,
                    Age = age,
                    Gender = gender
                };

                var prediction = predictionEngine.Predict(input);
                predictions.Add((days, prediction.PredictedWeight));
            }

            // Find when target will be reached
            var estimatedDays = EstimateDaysToGoal(
                (float)currentWeight,
                (float)targetWeight,
                predictions);

            // Calculate confidence level
            var confidence = CalculateConfidence(progressEntries, predictions);

            // Generate recommendations
            var recommendation = GenerateRecommendation(
                user.CurrentGoal.WeightGoal,
                (float)currentWeight,
                (float)targetWeight,
                averageDailyCalories,
                user.CurrentGoal.TDEE);

            // Check if realistic
            var weeklyChange = Math.Abs(predictions[0].weight - (float)currentWeight);
            var isRealistic = weeklyChange >= 0.2f && weeklyChange <= 1.5f;

            return new GoalPredictionResult
            {
                PredictedWeight = predictions.Last().weight,
                EstimatedDaysToGoal = estimatedDays,
                EstimatedAchievementDate = DateTime.UtcNow.AddDays(estimatedDays),
                ConfidenceLevel = confidence,
                Recommendation = recommendation,
                IsRealistic = isRealistic
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to predict goal achievement for user {UserId}", userId);
            return null;
        }
    }

    private TransformerChain<RegressionPredictionTransformer<FastTreeRegressionModelParameters>> TrainModel(
        List<ProgressEntry> progressEntries,
        List<MealLog> mealLogs,
        User user)
    {
        // Prepare training data
        var trainingData = new List<WeightPredictionInput>();
        
        var firstDate = progressEntries.First().RecordedAt;
        var age = user.GetAge() ?? 25;
        var gender = user.Gender == Gender.Male ? 1f : 0f;
        var activityLevel = (float)(user.CurrentGoal?.ActivityLevel ?? ActivityLevel.Moderate);

        foreach (var entry in progressEntries)
        {
            var dayNumber = (float)(entry.RecordedAt - firstDate).TotalDays;
            var avgCalories = CalculateAverageDailyCaloriesUpToDate(mealLogs, entry.RecordedAt);

            trainingData.Add(new WeightPredictionInput
            {
                DayNumber = dayNumber,
                CurrentWeight = (float)entry.Weight,
                AverageDailyCalories = avgCalories,
                ActivityLevel = activityLevel,
                Age = age,
                Gender = gender
            });
        }

        var data = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Build pipeline
        var pipeline = _mlContext.Transforms
            .Concatenate("Features",
                nameof(WeightPredictionInput.DayNumber),
                nameof(WeightPredictionInput.CurrentWeight),
                nameof(WeightPredictionInput.AverageDailyCalories),
                nameof(WeightPredictionInput.ActivityLevel),
                nameof(WeightPredictionInput.Age),
                nameof(WeightPredictionInput.Gender))
            .Append(_mlContext.Regression.Trainers.FastTree(
                labelColumnName: nameof(WeightPredictionInput.CurrentWeight),
                featureColumnName: "Features"));

        // Train model
        return pipeline.Fit(data);
    }

    private static float CalculateAverageDailyCalories(List<MealLog> mealLogs)
    {
        if (!mealLogs.Any()) return 2000f;

        var dailyCalories = mealLogs
            .GroupBy(m => m.LoggedAt.Date)
            .Select(g => g.Sum(m => m.FoodSnapshot.TotalCalories))
            .ToList();

        return dailyCalories.Any() ? (float)dailyCalories.Average() : 2000f;
    }

    private static float CalculateAverageDailyCaloriesUpToDate(
        List<MealLog> mealLogs,
        DateTime date)
    {
        var relevantLogs = mealLogs
            .Where(m => m.LoggedAt <= date)
            .ToList();

        return CalculateAverageDailyCalories(relevantLogs);
    }

    private static int EstimateDaysToGoal(
        float currentWeight,
        float targetWeight,
        List<(int days, float weight)> predictions)
    {
        var isLosing = targetWeight < currentWeight;

        foreach (var (days, weight) in predictions)
        {
            if (isLosing && weight <= targetWeight)
                return days;
            if (!isLosing && weight >= targetWeight)
                return days;
        }

        // Extrapolate if not found in predictions
        var lastPrediction = predictions.Last();
        var weeklyChange = (lastPrediction.weight - currentWeight) / (lastPrediction.days / 7f);
        
        if (Math.Abs(weeklyChange) < 0.01f)
            return 365; // Return 1 year if no change

        var remainingChange = targetWeight - lastPrediction.weight;
        var weeksNeeded = Math.Abs(remainingChange / weeklyChange);
        
        return lastPrediction.days + (int)(weeksNeeded * 7);
    }

    private static float CalculateConfidence(
        List<ProgressEntry> entries,
        List<(int days, float weight)> predictions)
    {
        // Base confidence on data quality
        float confidence = 50f;

        // More data = higher confidence
        if (entries.Count >= 10) confidence += 20f;
        else if (entries.Count >= 5) confidence += 10f;

        // Consistent tracking = higher confidence
        var avgInterval = entries.Count > 1
            ? entries.Zip(entries.Skip(1), (a, b) => (b.RecordedAt - a.RecordedAt).TotalDays).Average()
            : 7;

        if (avgInterval <= 3) confidence += 15f; // Daily tracking
        else if (avgInterval <= 7) confidence += 10f; // Weekly tracking

        // Realistic predictions = higher confidence
        var weeklyChange = Math.Abs(predictions[0].weight - (float)entries.Last().Weight);
        if (weeklyChange >= 0.2f && weeklyChange <= 1.0f)
            confidence += 15f;

        return Math.Min(confidence, 95f); // Max 95% confidence
    }

    private static string GenerateRecommendation(
        WeightGoal goal,
        float currentWeight,
        float targetWeight,
        float avgCalories,
        int targetCalories)
    {
        var calorieDiff = avgCalories - targetCalories;

        if (goal == WeightGoal.Lose)
        {
            if (calorieDiff > 300)
                return "You're eating too much. Reduce daily calories by ~300 to stay on track";
            if (calorieDiff < -500)
                return "You're eating too little. This might slow your metabolism. Increase calories slightly";
            return "Keep up your current calorie intake - you're on track!";
        }
        else if (goal == WeightGoal.Gain)
        {
            if (calorieDiff < -300)
                return "Increase your calorie intake by ~300 daily to reach your goal faster";
            if (calorieDiff > 500)
                return "You might be eating too much. Small surplus is better for lean gains";
            return "Your calorie intake looks good for muscle gain!";
        }

        return "Maintain your current habits to stay at your target weight";
    }
}