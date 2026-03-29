// Users API endpoints
import apiClient from "../client";
import { UserProfile, UpdateProfileRequest } from "../../types";

/**
 * Get current user profile
 */
export const getProfile = async (): Promise<UserProfile> => {
  const response = await apiClient.get<UserProfile>("/users/profile");
  return response.data;
};

/**
 * Update user profile
 */
export const updateProfile = async (
  data: UpdateProfileRequest
): Promise<void> => {
  await apiClient.put("/users/profile", data);
};

/**
 * Upload profile image
 */
export const uploadProfileImage = async (
  formData: FormData
): Promise<string> => {
  const response = await apiClient.post<{ imageUrl: string }>(
    "/users/profile/image",
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );
  return response.data.imageUrl;
};

/**
 * Delete profile image
 */
export const deleteProfileImage = async (): Promise<void> => {
  await apiClient.delete("/users/profile/image");
};
