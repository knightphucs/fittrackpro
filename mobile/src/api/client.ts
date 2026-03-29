// API Client - Axios instance with interceptors
import axios, {
  AxiosInstance,
  AxiosError,
  InternalAxiosRequestConfig,
} from "axios";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { config } from "../constants";
import { AuthResponse } from "../types";

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: config.api.baseUrl,
  timeout: config.api.timeout,
  headers: {
    "Content-Type": "application/json",
  },
});

// Cache refresh calls so parallel 401s share one refresh
let refreshPromise: Promise<string | null> | null = null;

const refreshAccessToken = async (): Promise<string | null> => {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    const storedRefreshToken = await AsyncStorage.getItem(
      config.storageKeys.refreshToken
    );

    if (!storedRefreshToken) {
      return null;
    }

    try {
      const response = await axios.post<AuthResponse>(
        `${config.api.baseUrl}/auth/refresh`,
        { refreshToken: storedRefreshToken },
        {
          headers: { "Content-Type": "application/json" },
          timeout: config.api.timeout,
        }
      );

      const { accessToken, refreshToken } = response.data;
      await AsyncStorage.setItem(config.storageKeys.accessToken, accessToken);
      await AsyncStorage.setItem(config.storageKeys.refreshToken, refreshToken);

      return accessToken;
    } catch (refreshError) {
      await AsyncStorage.multiRemove([
        config.storageKeys.accessToken,
        config.storageKeys.refreshToken,
        config.storageKeys.userInfo,
      ]);
      return null;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
};

// Request interceptor - Add auth token
apiClient.interceptors.request.use(
  async (requestConfig: InternalAxiosRequestConfig) => {
    const token = await AsyncStorage.getItem(config.storageKeys.accessToken);

    if (token && requestConfig.headers) {
      requestConfig.headers.Authorization = `Bearer ${token}`;
    }

    return requestConfig;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle errors globally
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as
      | (InternalAxiosRequestConfig & {
          _retry?: boolean;
        })
      | undefined;

    // Handle 401 Unauthorized - try refresh once
    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry
    ) {
      originalRequest._retry = true;

      const newAccessToken = await refreshAccessToken();

      if (newAccessToken) {
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        }
        return apiClient(originalRequest);
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
