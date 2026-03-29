// App color palette
export const colors = {
  // Primary colors
  primary: "#007AFF",
  primaryLight: "#a0c4ff",
  primaryDark: "#0056b3",

  // Secondary colors
  secondary: "#5856D6",
  accent: "#FF9500",

  // Semantic colors
  success: "#34C759",
  successLight: "#D1F2DC",
  warning: "#FF9500",
  error: "#FF3B30",
  errorLight: "#FFE5E5",
  info: "#5AC8FA",

  // Neutral colors
  white: "#FFFFFF",
  black: "#000000",
  background: "#F2F2F7",
  backgroundSecondary: "#E5E5EA",
  surface: "#FFFFFF",

  // Text colors
  textPrimary: "#1c1c1e",
  textSecondary: "#8e8e93",
  textTertiary: "#c7c7cc",
  textInverse: "#FFFFFF",

  // Border colors
  border: "#e5e5ea",
  borderLight: "#f2f2f7",
  divider: "#c6c6c8",

  // Macro colors (nutrition)
  protein: "#4ECDC4",
  carbs: "#FFD166",
  fat: "#FF6B6B",
  fiber: "#95D5B2",
  calories: "#FF6B6B",

  // Transparent
  transparent: "transparent",
  overlay: "rgba(0, 0, 0, 0.5)",
} as const;

export type ColorKey = keyof typeof colors;
