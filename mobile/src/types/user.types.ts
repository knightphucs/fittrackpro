// Types for User Profile
export type Gender = "Male" | "Female" | "Other" | "PreferNotToSay";
export type ActivityLevel =
  | "Sedentary"
  | "LightlyActive"
  | "ModeratelyActive"
  | "VeryActive"
  | "ExtremelyActive";
export type WeightGoal = "Lose" | "Maintain" | "Gain";

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  dateOfBirth?: string;
  gender?: Gender;
  height?: number; // cm
  currentWeight?: number; // kg
  targetWeight?: number; // kg
  activityLevel?: ActivityLevel;
  weightGoal?: WeightGoal;
  dailyCalorieTarget?: number;
  profileImageUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  dateOfBirth?: string;
  gender?: Gender;
  height?: number;
  currentWeight?: number;
  targetWeight?: number;
  activityLevel?: ActivityLevel;
  weightGoal?: WeightGoal;
  dailyCalorieTarget?: number;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
