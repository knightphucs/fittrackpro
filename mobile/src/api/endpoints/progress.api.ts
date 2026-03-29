// Progress API endpoints
import apiClient from "../client";
import {
  ProgressEntry,
  ProgressStatistics,
  ProgressPhoto,
  CreateProgressRequest,
} from "../../types";

export interface GetProgressHistoryParams {
  startDate?: string;
  endDate?: string;
}

export interface GetProgressPhotosParams {
  photoType?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Get progress history
 */
export const getHistory = async (
  params: GetProgressHistoryParams = {}
): Promise<ProgressEntry[]> => {
  const response = await apiClient.get<ProgressEntry[]>("/progress/history", {
    params,
  });
  return response.data;
};

/**
 * Get progress statistics
 */
export const getStatistics = async (
  days: number = 30
): Promise<ProgressStatistics> => {
  const response = await apiClient.get<ProgressStatistics>(
    "/progress/statistics",
    { params: { days } }
  );
  return response.data;
};

/**
 * Get progress photos
 */
export const getPhotos = async (
  params: GetProgressPhotosParams = {}
): Promise<ProgressPhoto[]> => {
  const response = await apiClient.get<ProgressPhoto[]>("/progress/photos", {
    params,
  });
  return response.data;
};

/**
 * Create progress entry (log weight)
 */
export const createProgressEntry = async (
  data: CreateProgressRequest
): Promise<ProgressEntry> => {
  const response = await apiClient.post<ProgressEntry>("/progress", data);
  return response.data;
};

/**
 * Update progress entry
 */
export const updateProgressEntry = async (
  id: string,
  data: Partial<CreateProgressRequest>
): Promise<ProgressEntry> => {
  const response = await apiClient.put<ProgressEntry>(`/progress/${id}`, data);
  return response.data;
};

/**
 * Delete progress entry
 */
export const deleteProgressEntry = async (id: string): Promise<void> => {
  await apiClient.delete(`/progress/${id}`);
};

/**
 * Upload progress photo
 */
export const uploadProgressPhoto = async (
  photo: FormData
): Promise<ProgressPhoto> => {
  const response = await apiClient.post<ProgressPhoto>(
    `/progress/photos`,
    photo,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );
  return response.data;
};

/**
 * Delete progress photo
 */
export const deleteProgressPhoto = async (photoId: string): Promise<void> => {
  await apiClient.delete(`/progress/photos/${photoId}`);
};
