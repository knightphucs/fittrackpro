// Types for Workouts
import { Exercise } from "./exercise.types";

export type WorkoutStatus = "Active" | "Completed" | "Cancelled";

export interface ExerciseSet {
  id: string;
  setNumber: number;
  reps?: number;
  weight?: number;
  duration?: number;
  distance?: number;
  notes?: string;
  completedAt?: string;
}

export interface WorkoutExercise {
  id: string;
  exercise: Exercise;
  orderIndex: number;
  sets: ExerciseSet[];
  notes?: string;
}

export interface WorkoutSession {
  id: string;
  title: string;
  notes?: string;
  startedAt: string;
  endedAt?: string;
  durationMinutes: number;
  totalCaloriesBurned: number;
  status: WorkoutStatus;
  exercises: WorkoutExercise[];
}

export interface StartWorkoutRequest {
  title: string;
  notes?: string;
}

export interface LogExerciseRequest {
  exerciseId: string;
  sets: {
    reps?: number;
    weight?: number;
    duration?: number;
    distance?: number;
    notes?: string;
  }[];
  notes?: string;
}

export interface CompleteWorkoutRequest {
  rating?: number;
  notes?: string;
}

export interface WorkoutSummary {
  totalWorkouts: number;
  totalDurationMinutes: number;
  totalCaloriesBurned: number;
  avgWorkoutDuration: number;
  workoutsThisWeek: number;
  mostFrequentExercises: string[];
}

export interface PersonalRecord {
  id: string;
  exerciseId: string;
  exerciseName: string;
  recordType: "MaxWeight" | "MaxReps" | "MaxDuration" | "MaxDistance";
  value: number;
  achievedAt: string;
  previousValue?: number;
}

export interface GetWorkoutHistoryParams {
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
}
