// Workouts API endpoints
import apiClient from "../client";
import {
  WorkoutSession,
  WorkoutSummary,
  PersonalRecord,
  StartWorkoutRequest,
  LogExerciseRequest,
  CompleteWorkoutRequest,
  GetWorkoutHistoryParams,
  PaginatedResult,
} from "../../types";

/**
 * Start a new workout session
 */
export const startWorkout = async (
  data: StartWorkoutRequest
): Promise<WorkoutSession> => {
  const response = await apiClient.post<WorkoutSession>(
    "/workouts/start",
    data
  );
  return response.data;
};

/**
 * Log an exercise to current workout
 */
export const logExercise = async (
  workoutId: string,
  data: LogExerciseRequest
): Promise<WorkoutSession> => {
  const response = await apiClient.post<WorkoutSession>(
    `/workouts/${workoutId}/exercises`,
    data
  );
  return response.data;
};

/**
 * Complete a workout session
 */
export const completeWorkout = async (
  workoutId: string,
  data?: CompleteWorkoutRequest
): Promise<WorkoutSession> => {
  const response = await apiClient.post<WorkoutSession>(
    `/workouts/${workoutId}/complete`,
    data || {}
  );
  return response.data;
};

/**
 * Get workout history
 */
export const getWorkoutHistory = async (
  params: GetWorkoutHistoryParams = {}
): Promise<PaginatedResult<WorkoutSession>> => {
  const response = await apiClient.get<PaginatedResult<WorkoutSession>>(
    "/workouts/history",
    { params }
  );
  return response.data;
};

/**
 * Get active workout session
 */
export const getActiveWorkout = async (): Promise<WorkoutSession | null> => {
  try {
    const response = await apiClient.get<WorkoutSession>("/workouts/active");
    return response.data;
  } catch (error: any) {
    if (error.response?.status === 404) {
      return null;
    }
    throw error;
  }
};

/**
 * Get workout summary/statistics
 */
export const getWorkoutSummary = async (
  startDate?: string,
  endDate?: string
): Promise<WorkoutSummary> => {
  const params: Record<string, string> = {};
  if (startDate) params.startDate = startDate;
  if (endDate) params.endDate = endDate;

  const response = await apiClient.get<WorkoutSummary>("/workouts/summary", {
    params,
  });
  return response.data;
};

/**
 * Get personal records
 */
export const getPersonalRecords = async (): Promise<PersonalRecord[]> => {
  const response = await apiClient.get<PersonalRecord[]>(
    "/workouts/personal-records"
  );
  return response.data;
};

/**
 * Delete a workout
 */
export const deleteWorkout = async (workoutId: string): Promise<void> => {
  await apiClient.delete(`/workouts/${workoutId}`);
};
