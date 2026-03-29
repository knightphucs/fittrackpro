// App configuration constants
import { Platform } from "react-native";

// API Configuration
const NGROK_URL = "https://d8cbb5fd0fe8.ngrok-free.app";

export const config = {
  // API
  api: {
    baseUrl:
      Platform.OS === "android" ? `${NGROK_URL}/api` : `${NGROK_URL}/api`,
    timeout: 30000, // 30 seconds
  },

  // Pagination defaults
  pagination: {
    defaultPageSize: 10,
    maxPageSize: 50,
  },

  // Storage keys
  storageKeys: {
    accessToken: "accessToken",
    refreshToken: "refreshToken",
    userInfo: "userInfo",
    onboardingComplete: "onboardingComplete",
    theme: "theme",
  },

  // Feature flags
  features: {
    enableProgressPhotos: true,
    enableSocialSharing: false,
    enableNotifications: true,
    enableDarkMode: true,
  },

  // Validation
  validation: {
    minPasswordLength: 8,
    maxNameLength: 50,
    maxNotesLength: 500,
  },

  // Image configuration
  images: {
    maxSize: 5 * 1024 * 1024, // 5MB
    allowedTypes: ["image/jpeg", "image/png", "image/webp"],
    placeholderUrl: "https://via.placeholder.com/150",
  },
} as const;

export default config;
