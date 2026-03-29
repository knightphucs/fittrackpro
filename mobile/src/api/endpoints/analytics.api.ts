// Analytics API endpoints
import apiClient from "../client";
import {
  DashboardData,
  WeeklyReport,
  NutritionTrends,
  GoalPrediction,
} from "../../types";

/**
 * Get dashboard summary
 * Maps backend payload to the UI-friendly DashboardData shape
 */
export const getDashboard = async (): Promise<DashboardData> => {
  const response = await apiClient.get("/analytics/dashboard");
  const data = response.data as any;

  const weightProgress = data?.progressSummary
    ? {
        current: data.progressSummary.currentWeight ?? 0,
        target: data.progressSummary.targetWeight ?? 0,
        change: data.progressSummary.weightChange ?? 0,
      }
    : undefined;

  return {
    todayCalories: data?.todaysSummary?.caloriesConsumed ?? 0,
    targetCalories: data?.todaysSummary?.caloriesTarget ?? 0,
    caloriesRemaining: data?.todaysSummary?.caloriesRemaining ?? 0,
    todayProtein: data?.todaysSummary?.proteinGrams ?? 0,
    todayCarbs: data?.todaysSummary?.carbsGrams ?? 0,
    todayFat: data?.todaysSummary?.fatGrams ?? 0,
    streakDays: data?.streaks?.currentStreak ?? 0,
    workoutsThisWeek: data?.weeklyTrends?.totalWorkouts ?? 0,
    weightProgress,
  };
};

/**
 * Get weekly report
 */
export const getWeeklyReport = async (
  startDate?: string
): Promise<WeeklyReport> => {
  const params = startDate ? { startDate } : {};
  const response = await apiClient.get<WeeklyReport>(
    "/analytics/weekly-report",
    { params }
  );
  return response.data;
};

/**
 * Get nutrition trends
 */
export const getNutritionTrends = async (
  days: number = 30
): Promise<NutritionTrends> => {
  const response = await apiClient.get<NutritionTrends>(
    "/analytics/nutrition-trends",
    { params: { days } }
  );
  return response.data;
};

/**
 * Get goal prediction
 */
export const getGoalPrediction = async (): Promise<GoalPrediction> => {
  const response = await apiClient.get<GoalPrediction>(
    "/analytics/goal-prediction"
  );
  return response.data;
};

/**
 * Get monthly summary
 */
export const getMonthlySummary = async (
  year?: number,
  month?: number
): Promise<any> => {
  const params: Record<string, number> = {};
  if (year) params.year = year;
  if (month) params.month = month;

  const response = await apiClient.get("/analytics/monthly-summary", {
    params,
  });
  return response.data;
};
