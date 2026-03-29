// Goals API endpoints
import apiClient from "../client";
import { Goal, CreateGoalRequest } from "../../types";

/**
 * Get current active goal for user
 */
export const getCurrentGoal = async (): Promise<Goal | null> => {
  try {
    const response = await apiClient.get<Goal>("/goals/current");
    return response.data;
  } catch (error: any) {
    if (error.response?.status === 404) {
      return null;
    }
    throw error;
  }
};

/**
 * Get all goals for user
 */
export const getGoals = async (): Promise<Goal[]> => {
  const response = await apiClient.get<Goal[]>("/goals");
  return response.data;
};

/**
 * Create new goal
 */
export const createGoal = async (data: CreateGoalRequest): Promise<Goal> => {
  const response = await apiClient.post<Goal>("/goals", data);
  return response.data;
};

/**
 * Update goal
 */
export const updateGoal = async (
  id: string,
  data: Partial<CreateGoalRequest>
): Promise<Goal> => {
  const response = await apiClient.put<Goal>(`/goals/${id}`, data);
  return response.data;
};

/**
 * Delete goal
 */
export const deleteGoal = async (id: string): Promise<void> => {
  await apiClient.delete(`/goals/${id}`);
};

/**
 * Set goal as active
 */
export const setActiveGoal = async (id: string): Promise<Goal> => {
  const response = await apiClient.patch<Goal>(`/goals/${id}/activate`);
  return response.data;
};
