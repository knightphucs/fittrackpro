// Types for Exercises
export type ExerciseCategory =
  | "Strength"
  | "Cardio"
  | "Flexibility"
  | "Plyometric"
  | "Powerlifting"
  | "Olympic"
  | "Calisthenics"
  | "Sports";

export type MuscleGroup =
  | "Chest"
  | "Back"
  | "Shoulders"
  | "Biceps"
  | "Triceps"
  | "Forearms"
  | "Core"
  | "Quadriceps"
  | "Hamstrings"
  | "Glutes"
  | "Calves"
  | "FullBody";

export type EquipmentType =
  | "None"
  | "Barbell"
  | "Dumbbell"
  | "Kettlebell"
  | "Machine"
  | "Cable"
  | "Bodyweight"
  | "Bands"
  | "Other";

export type DifficultyLevel =
  | "Beginner"
  | "Intermediate"
  | "Advanced"
  | "Expert";

export interface Exercise {
  id: string;
  name: string;
  nameVi?: string;
  description?: string;
  category: ExerciseCategory;
  primaryMuscle: MuscleGroup;
  secondaryMuscles: MuscleGroup[];
  equipment: EquipmentType;
  difficulty: DifficultyLevel;
  videoUrl?: string;
  imageUrl?: string;
  instructions?: string;
  isUserCreated: boolean;
}

export interface SearchExercisesParams {
  searchTerm?: string;
  category?: ExerciseCategory;
  muscleGroup?: MuscleGroup;
  equipment?: EquipmentType;
  difficulty?: DifficultyLevel;
  pageNumber?: number;
  pageSize?: number;
}
