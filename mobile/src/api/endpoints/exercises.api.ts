// Exercises API endpoints
import apiClient from "../client";
import { config } from "../../constants";
import {
  Exercise,
  SearchExercisesParams,
  ExerciseCategory,
  MuscleGroup,
  EquipmentType,
  DifficultyLevel,
  PaginatedResult,
} from "../../types";

/**
 * Search exercises with filters
 */
export const searchExercises = async (
  params: SearchExercisesParams = {}
): Promise<PaginatedResult<Exercise>> => {
  const {
    pageNumber = 1,
    pageSize = config.pagination.defaultPageSize,
    ...filters
  } = params;

  const queryParams: Record<string, string | number> = {
    pageNumber,
    pageSize,
  };

  if (filters.searchTerm) queryParams.searchTerm = filters.searchTerm;
  if (filters.category) queryParams.category = filters.category;
  if (filters.muscleGroup) queryParams.muscleGroup = filters.muscleGroup;
  if (filters.equipment) queryParams.equipment = filters.equipment;
  if (filters.difficulty) queryParams.difficulty = filters.difficulty;

  const response = await apiClient.get<PaginatedResult<Exercise>>(
    "/exercises",
    { params: queryParams }
  );
  return response.data;
};

/**
 * Get exercise categories
 */
export const getCategories = async (): Promise<ExerciseCategory[]> => {
  const response = await apiClient.get<ExerciseCategory[]>(
    "/exercises/categories"
  );
  return response.data;
};

/**
 * Get muscle groups
 */
export const getMuscleGroups = async (): Promise<MuscleGroup[]> => {
  const response = await apiClient.get<MuscleGroup[]>(
    "/exercises/muscle-groups"
  );
  return response.data;
};

/**
 * Get equipment types
 */
export const getEquipmentTypes = async (): Promise<EquipmentType[]> => {
  const response = await apiClient.get<EquipmentType[]>(
    "/exercises/equipment-types"
  );
  return response.data;
};

/**
 * Get difficulty levels
 */
export const getDifficultyLevels = async (): Promise<DifficultyLevel[]> => {
  const response = await apiClient.get<DifficultyLevel[]>(
    "/exercises/difficulty-levels"
  );
  return response.data;
};
