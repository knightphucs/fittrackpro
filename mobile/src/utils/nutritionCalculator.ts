// Nutrition calculation utilities
import { Food, DailyMeals, Goal } from "../types";

export interface NutritionSummary {
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  fiber?: number;
  sugar?: number;
}

export interface NutritionGoalProgress {
  calories: { current: number; target: number; percentage: number };
  protein: { current: number; target: number; percentage: number };
  carbs: { current: number; target: number; percentage: number };
  fat: { current: number; target: number; percentage: number };
}

/**
 * Calculate nutrition for a food item with servings
 */
export const calculateFoodNutrition = (
  food: Food,
  servings: number = 1
): NutritionSummary => {
  return {
    calories: Math.round(food.calories * servings),
    protein: Math.round(food.protein * servings * 10) / 10,
    carbs: Math.round(food.carbs * servings * 10) / 10,
    fat: Math.round(food.fat * servings * 10) / 10,
    fiber: food.fiber ? Math.round(food.fiber * servings * 10) / 10 : undefined,
    sugar: food.sugar ? Math.round(food.sugar * servings * 10) / 10 : undefined,
  };
};

/**
 * Calculate total nutrition from daily meals
 */
export const calculateDailyNutrition = (
  dailyMeals: DailyMeals
): NutritionSummary => {
  return {
    calories: dailyMeals.totalCalories,
    protein: dailyMeals.totalProtein,
    carbs: dailyMeals.totalCarbs,
    fat: dailyMeals.totalFat,
  };
};

/**
 * Calculate progress towards nutrition goals
 */
export const calculateGoalProgress = (
  current: NutritionSummary,
  goal: Goal
): NutritionGoalProgress => {
  const calcPercentage = (current: number, target: number) => {
    if (target === 0) return 0;
    return Math.min(Math.round((current / target) * 100), 100);
  };

  return {
    calories: {
      current: current.calories,
      target: goal.targetCalories,
      percentage: calcPercentage(current.calories, goal.targetCalories),
    },
    protein: {
      current: current.protein,
      target: goal.targetProtein || 0,
      percentage: calcPercentage(current.protein, goal.targetProtein || 0),
    },
    carbs: {
      current: current.carbs,
      target: goal.targetCarbs || 0,
      percentage: calcPercentage(current.carbs, goal.targetCarbs || 0),
    },
    fat: {
      current: current.fat,
      target: goal.targetFat || 0,
      percentage: calcPercentage(current.fat, goal.targetFat || 0),
    },
  };
};

/**
 * Calculate calories from macros
 * Protein: 4 cal/g, Carbs: 4 cal/g, Fat: 9 cal/g
 */
export const calculateCaloriesFromMacros = (
  protein: number,
  carbs: number,
  fat: number
): number => {
  return Math.round(protein * 4 + carbs * 4 + fat * 9);
};

/**
 * Calculate macro percentages
 */
export const calculateMacroPercentages = (
  protein: number,
  carbs: number,
  fat: number
): { protein: number; carbs: number; fat: number } => {
  const totalCalories = calculateCaloriesFromMacros(protein, carbs, fat);

  if (totalCalories === 0) {
    return { protein: 0, carbs: 0, fat: 0 };
  }

  return {
    protein: Math.round(((protein * 4) / totalCalories) * 100),
    carbs: Math.round(((carbs * 4) / totalCalories) * 100),
    fat: Math.round(((fat * 9) / totalCalories) * 100),
  };
};

/**
 * Format calories display
 */
export const formatCalories = (calories: number): string => {
  return `${Math.round(calories)} kcal`;
};

/**
 * Format macro display
 */
export const formatMacro = (value: number, unit: string = "g"): string => {
  return `${Math.round(value * 10) / 10}${unit}`;
};
