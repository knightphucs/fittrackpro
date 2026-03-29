// Types for Analytics & Dashboard
export interface DashboardData {
  todayCalories: number;
  targetCalories: number;
  caloriesRemaining: number;
  todayProtein: number;
  todayCarbs: number;
  todayFat: number;
  streakDays: number;
  workoutsThisWeek: number;
  weightProgress?: {
    current: number;
    target: number;
    change: number;
  };
}

export interface WeeklyReport {
  startDate: string;
  endDate: string;
  totalDays: number;
  startWeight: number;
  endWeight: number;
  weightChange: number;
  onTrack: boolean;
  nutrition: NutritionSummary;
  activity: ActivitySummary;
  achievements: string[];
  recommendations: string[];
}

export interface NutritionSummary {
  avgCalories: number;
  avgProtein: number;
  avgCarbs: number;
  avgFat: number;
  totalMealsLogged: number;
  daysWithMeals: number;
  calorieGoalHitDays: number;
}

export interface ActivitySummary {
  totalWorkouts: number;
  totalDurationMinutes: number;
  totalCaloriesBurned: number;
  avgWorkoutDuration: number;
  mostActiveDay: string;
}

export interface NutritionTrends {
  period: string;
  dailyData: DailyNutrition[];
  macroTrends: MacroTrends;
  calorieTrend: CalorieTrend;
  topFoods: TopFood[];
  mealDistribution: MealDistribution;
}

export interface DailyNutrition {
  date: string;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  mealsLogged: number;
}

export interface MacroTrends {
  avgProteinPercentage: number;
  avgCarbsPercentage: number;
  avgFatPercentage: number;
  proteinTrend: "increasing" | "decreasing" | "stable";
  carbsTrend: "increasing" | "decreasing" | "stable";
  fatTrend: "increasing" | "decreasing" | "stable";
}

export interface CalorieTrend {
  avgDailyCalories: number;
  targetCalories: number;
  trend: "increasing" | "decreasing" | "stable";
  variance: number;
}

export interface TopFood {
  foodId: string;
  foodName: string;
  timesConsumed: number;
  totalCalories: number;
}

export interface MealDistribution {
  breakfast: number;
  lunch: number;
  dinner: number;
  snack: number;
}

export interface GoalPrediction {
  currentWeight: number;
  targetWeight: number;
  predictedDate: string;
  daysRemaining: number;
  onTrack: boolean;
  weeklyProgress: number;
  recommendations: string[];
}
