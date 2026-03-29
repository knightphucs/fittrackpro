// Custom hook for foods data
import { useState, useCallback } from "react";
import { foodsApi, GetFoodsParams } from "../api/endpoints/foods.api";
import { Food, PaginatedResult } from "../types";

interface UseFoodsReturn {
  foods: Food[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  hasMore: boolean;
  fetchFoods: (params?: GetFoodsParams) => Promise<void>;
  searchFoods: (searchTerm: string) => Promise<void>;
  loadMore: () => Promise<void>;
  refresh: () => Promise<void>;
}

export const useFoods = (): UseFoodsReturn => {
  const [foods, setFoods] = useState<Food[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [hasMore, setHasMore] = useState(true);
  const [currentParams, setCurrentParams] = useState<GetFoodsParams>({});

  const fetchFoods = useCallback(async (params: GetFoodsParams = {}) => {
    setLoading(true);
    setError(null);

    try {
      const result = await foodsApi.getFoods({ ...params, pageNumber: 1 });
      setFoods(result.items);
      setTotalCount(result.totalCount);
      setHasMore(result.hasNextPage);
      setCurrentPage(1);
      setCurrentParams(params);
    } catch (err: any) {
      setError(err.message || "Failed to fetch foods");
    } finally {
      setLoading(false);
    }
  }, []);

  const searchFoods = useCallback(
    async (searchTerm: string) => {
      await fetchFoods({ searchTerm: searchTerm || undefined });
    },
    [fetchFoods]
  );

  const loadMore = useCallback(async () => {
    if (loading || !hasMore) return;

    setLoading(true);
    try {
      const nextPage = currentPage + 1;
      const result = await foodsApi.getFoods({
        ...currentParams,
        pageNumber: nextPage,
      });

      setFoods((prev) => [...prev, ...result.items]);
      setCurrentPage(nextPage);
      setHasMore(result.hasNextPage);
    } catch (err: any) {
      setError(err.message || "Failed to load more foods");
    } finally {
      setLoading(false);
    }
  }, [loading, hasMore, currentPage, currentParams]);

  const refresh = useCallback(async () => {
    await fetchFoods(currentParams);
  }, [fetchFoods, currentParams]);

  return {
    foods,
    loading,
    error,
    totalCount,
    hasMore,
    fetchFoods,
    searchFoods,
    loadMore,
    refresh,
  };
};

export default useFoods;
