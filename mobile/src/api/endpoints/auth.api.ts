// Authentication API endpoints
import AsyncStorage from "@react-native-async-storage/async-storage";
import apiClient from "../client";
import { config } from "../../constants";
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserInfo,
} from "../../types";

/**
 * Login user with email and password
 */
export const login = async (
  credentials: LoginRequest
): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>(
    "/auth/login",
    credentials
  );
  const data = response.data;

  // Store tokens
  await AsyncStorage.setItem(config.storageKeys.accessToken, data.accessToken);
  await AsyncStorage.setItem(
    config.storageKeys.refreshToken,
    data.refreshToken
  );

  // Store user info
  const userInfo: UserInfo = {
    id: data.userId,
    email: data.email,
    firstName: data.firstName,
    lastName: data.lastName,
  };
  await AsyncStorage.setItem(
    config.storageKeys.userInfo,
    JSON.stringify(userInfo)
  );

  return data;
};

/**
 * Register new user
 */
export const register = async (
  data: RegisterRequest
): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>("/auth/register", data);
  return response.data;
};

/**
 * Logout user and clear stored tokens
 */
export const logout = async (): Promise<void> => {
  await AsyncStorage.multiRemove([
    config.storageKeys.accessToken,
    config.storageKeys.refreshToken,
    config.storageKeys.userInfo,
  ]);
};

/**
 * Get stored user info
 */
export const getStoredUserInfo = async (): Promise<UserInfo | null> => {
  const userInfoString = await AsyncStorage.getItem(
    config.storageKeys.userInfo
  );
  return userInfoString ? JSON.parse(userInfoString) : null;
};

/**
 * Check if user is authenticated
 */
export const isAuthenticated = async (): Promise<boolean> => {
  const token = await AsyncStorage.getItem(config.storageKeys.accessToken);
  return !!token;
};

/**
 * Refresh access token (TODO: implement backend endpoint)
 */
export const refreshToken = async (
  refreshToken: string
): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>("/auth/refresh", {
    refreshToken,
  });
  const data = response.data;

  await AsyncStorage.setItem(config.storageKeys.accessToken, data.accessToken);
  await AsyncStorage.setItem(
    config.storageKeys.refreshToken,
    data.refreshToken
  );

  return data;
};
