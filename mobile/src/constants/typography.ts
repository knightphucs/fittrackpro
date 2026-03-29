// Typography constants
import { Platform } from "react-native";

const fontFamily = Platform.select({
  ios: {
    regular: "System",
    medium: "System",
    semiBold: "System",
    bold: "System",
  },
  android: {
    regular: "Roboto",
    medium: "Roboto-Medium",
    semiBold: "Roboto-Medium",
    bold: "Roboto-Bold",
  },
});

export const typography = {
  // Font families
  fontFamily,

  // Font sizes
  fontSize: {
    xs: 10,
    sm: 12,
    md: 14,
    lg: 16,
    xl: 18,
    xxl: 20,
    xxxl: 24,
    title: 28,
    hero: 32,
  },

  // Font weights
  fontWeight: {
    regular: "400" as const,
    medium: "500" as const,
    semiBold: "600" as const,
    bold: "700" as const,
  },

  // Line heights
  lineHeight: {
    tight: 1.2,
    normal: 1.4,
    relaxed: 1.6,
  },

  // Pre-defined text styles
  styles: {
    hero: {
      fontSize: 32,
      fontWeight: "700" as const,
      lineHeight: 38,
    },
    title: {
      fontSize: 28,
      fontWeight: "700" as const,
      lineHeight: 34,
    },
    heading: {
      fontSize: 20,
      fontWeight: "600" as const,
      lineHeight: 26,
    },
    subheading: {
      fontSize: 18,
      fontWeight: "600" as const,
      lineHeight: 24,
    },
    body: {
      fontSize: 16,
      fontWeight: "400" as const,
      lineHeight: 22,
    },
    bodyBold: {
      fontSize: 16,
      fontWeight: "600" as const,
      lineHeight: 22,
    },
    caption: {
      fontSize: 14,
      fontWeight: "400" as const,
      lineHeight: 20,
    },
    small: {
      fontSize: 12,
      fontWeight: "400" as const,
      lineHeight: 16,
    },
    tiny: {
      fontSize: 10,
      fontWeight: "400" as const,
      lineHeight: 14,
    },
  },
} as const;
