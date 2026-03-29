// Foods API endpoints
import apiClient from "../client";
import { config } from "../../constants";
import {
  Food,
  FoodDetail,
  PaginatedResult,
  PaginationParams,
} from "../../types";

export interface GetFoodsParams extends PaginationParams {
  searchTerm?: string;
  category?: string;
}

/**
 * Get paginated list of foods
 */
export const getFoods = async (
  params: GetFoodsParams = {}
): Promise<PaginatedResult<Food>> => {
  const {
    pageNumber = 1,
    pageSize = config.pagination.defaultPageSize,
    searchTerm,
    category,
  } = params;

  const queryParams: Record<string, string | number> = {
    pageNumber,
    pageSize,
  };

  if (searchTerm) queryParams.searchTerm = searchTerm;
  if (category) queryParams.category = category;

  const response = await apiClient.get<PaginatedResult<Food>>("/foods", {
    params: queryParams,
  });
  return response.data;
};

/**
 * Get food by ID
 */
export const getFoodById = async (id: string): Promise<FoodDetail> => {
  const response = await apiClient.get<FoodDetail>(`/foods/${id}`);
  return response.data;
};

/**
 * Search foods by name
 */
export const searchFoods = async (
  searchTerm: string,
  limit: number = 10
): Promise<Food[]> => {
  const response = await apiClient.get<PaginatedResult<Food>>("/foods", {
    params: {
      searchTerm,
      pageSize: limit,
      pageNumber: 1,
    },
  });
  return response.data.items;
};

/**
 * Get food categories
 */
export const getCategories = async (): Promise<string[]> => {
  const response = await apiClient.get<string[]>("/foods/categories");
  return response.data;
};

/**
 * Get recently used foods for current user
 */
export const getRecentFoods = async (limit: number = 10): Promise<Food[]> => {
  const response = await apiClient.get<Food[]>("/foods/recent", {
    params: { limit },
  });
  return response.data;
};

export const foodsApi = {
  getFoods,
  getFoodById,
  searchFoods,
  getCategories,
  getRecentFoods,
};
