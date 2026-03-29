// Generic Button Component
import React from "react";
import {
  TouchableOpacity,
  Text,
  StyleSheet,
  ActivityIndicator,
  ViewStyle,
  TextStyle,
} from "react-native";
import { colors, spacing, borderRadius, typography } from "../../constants";

type ButtonVariant = "primary" | "secondary" | "outline" | "ghost";
type ButtonSize = "small" | "medium" | "large";

interface ButtonProps {
  title: string;
  onPress: () => void;
  variant?: ButtonVariant;
  size?: ButtonSize;
  disabled?: boolean;
  loading?: boolean;
  style?: ViewStyle;
  textStyle?: TextStyle;
  icon?: React.ReactNode;
}

const Button: React.FC<ButtonProps> = ({
  title,
  onPress,
  variant = "primary",
  size = "medium",
  disabled = false,
  loading = false,
  style,
  textStyle,
  icon,
}) => {
  const isDisabled = disabled || loading;

  const buttonStyles = [
    styles.base,
    styles[variant],
    styles[size],
    isDisabled && styles.disabled,
    isDisabled && styles[`${variant}Disabled`],
    style,
  ];

  const textStyles = [
    styles.text,
    styles[`${variant}Text`],
    styles[`${size}Text`],
    isDisabled && styles.disabledText,
    textStyle,
  ];

  return (
    <TouchableOpacity
      style={buttonStyles}
      onPress={onPress}
      disabled={isDisabled}
      activeOpacity={0.8}
    >
      {loading ? (
        <ActivityIndicator
          color={variant === "primary" ? colors.white : colors.primary}
          size="small"
        />
      ) : (
        <>
          {icon}
          <Text style={textStyles}>{title}</Text>
        </>
      )}
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  // Base styles
  base: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    borderRadius: borderRadius.sm,
  },

  // Variants
  primary: {
    backgroundColor: colors.primary,
  },
  secondary: {
    backgroundColor: colors.secondary,
  },
  outline: {
    backgroundColor: "transparent",
    borderWidth: 1,
    borderColor: colors.primary,
  },
  ghost: {
    backgroundColor: "transparent",
  },

  // Sizes
  small: {
    height: 36,
    paddingHorizontal: spacing.md,
  },
  medium: {
    height: 44,
    paddingHorizontal: spacing.lg,
  },
  large: {
    height: 52,
    paddingHorizontal: spacing.xl,
  },

  // Disabled states
  disabled: {
    opacity: 0.6,
  },
  primaryDisabled: {
    backgroundColor: colors.primaryLight,
  },
  secondaryDisabled: {},
  outlineDisabled: {
    borderColor: colors.border,
  },
  ghostDisabled: {},

  // Text styles
  text: {
    fontWeight: "600",
  },
  primaryText: {
    color: colors.white,
  },
  secondaryText: {
    color: colors.white,
  },
  outlineText: {
    color: colors.primary,
  },
  ghostText: {
    color: colors.primary,
  },

  // Text sizes
  smallText: {
    fontSize: typography.fontSize.sm,
  },
  mediumText: {
    fontSize: typography.fontSize.md,
  },
  largeText: {
    fontSize: typography.fontSize.lg,
  },

  disabledText: {
    color: colors.textTertiary,
  },
});

export default Button;
