// Types for Meal Logs
import { Food } from "./food.types";

export interface MealLog {
  id: string;
  userId?: string;
  foodId: string;
  foodName: string;
  foodNameVi?: string;
  food?: Food;
  mealType: MealType;
  servings: number;
  servingSize: number;
  servingUnit: string;
  servingMultiplier: number;
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
  loggedAt: string;
  notes?: string;
}

export type MealType = "Breakfast" | "Lunch" | "Dinner" | "Snack";

export interface CreateMealLogRequest {
  foodId: string;
  mealType: MealType;
  servings: number;
  loggedAt?: string;
  notes?: string;
}

export interface DailySummary {
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
  targetCalories: number;
  targetProtein: number;
  targetCarbs: number;
  targetFat: number;
  caloriesRemaining: number;
  proteinPercentage: number;
  carbsPercentage: number;
  fatPercentage: number;
}

export interface DailyMeals {
  date: string;
  meals: MealLog[];
  summary: DailySummary;
}
