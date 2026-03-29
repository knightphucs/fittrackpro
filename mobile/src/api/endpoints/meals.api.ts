// Meal Logs API endpoints
import apiClient from "../client";
import {
  MealLog,
  CreateMealLogRequest,
  DailyMeals,
  PaginatedResult,
  PaginationParams,
} from "../../types";

export interface GetMealLogsParams extends PaginationParams {
  date?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Get meal logs for current user
 */
export const getMealLogs = async (
  params: GetMealLogsParams = {}
): Promise<PaginatedResult<MealLog>> => {
  const response = await apiClient.get<PaginatedResult<MealLog>>("/meallogs", {
    params,
  });
  return response.data;
};

/**
 * Get daily meals summary
 */
export const getDailyMeals = async (date: string): Promise<DailyMeals> => {
  const response = await apiClient.get<DailyMeals>("/meallogs/daily", {
    params: { date },
  });
  return response.data;
};

/**
 * Create new meal log
 */
export const createMealLog = async (
  data: CreateMealLogRequest
): Promise<MealLog> => {
  const response = await apiClient.post<MealLog>("/meallogs", data);
  return response.data;
};

/**
 * Update meal log
 */
export const updateMealLog = async (
  id: string,
  data: Partial<CreateMealLogRequest>
): Promise<MealLog> => {
  const response = await apiClient.put<MealLog>(`/meallogs/${id}`, data);
  return response.data;
};

/**
 * Delete meal log
 */
export const deleteMealLog = async (id: string): Promise<void> => {
  await apiClient.delete(`/meallogs/${id}`);
};

/**
 * Get today's meals
 */
export const getTodayMeals = async (): Promise<DailyMeals> => {
  const today = new Date().toISOString().split("T")[0];
  return getDailyMeals(today);
};
