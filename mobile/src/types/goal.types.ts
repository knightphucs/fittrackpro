// Types for Goals
export interface Goal {
  id: string;
  userId: string;
  goalType: GoalType;
  targetCalories: number;
  targetProtein?: number;
  targetCarbs?: number;
  targetFat?: number;
  targetWeight?: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
}

export type GoalType =
  | "WeightLoss"
  | "WeightGain"
  | "Maintenance"
  | "MuscleGain";

export interface CreateGoalRequest {
  goalType: GoalType;
  targetCalories: number;
  targetProtein?: number;
  targetCarbs?: number;
  targetFat?: number;
  targetWeight?: number;
  startDate: string;
  endDate?: string;
}
