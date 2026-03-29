// Input Component
import React, { forwardRef } from "react";
import {
  View,
  TextInput,
  Text,
  StyleSheet,
  TextInputProps,
  ViewStyle,
} from "react-native";
import { colors, spacing, borderRadius, typography } from "../../constants";

interface InputProps extends TextInputProps {
  label?: string;
  error?: string;
  containerStyle?: ViewStyle;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

const Input = forwardRef<TextInput, InputProps>(
  (
    { label, error, containerStyle, leftIcon, rightIcon, style, ...props },
    ref
  ) => {
    return (
      <View style={[styles.container, containerStyle]}>
        {label && <Text style={styles.label}>{label}</Text>}

        <View style={[styles.inputContainer, error && styles.inputError]}>
          {leftIcon && <View style={styles.leftIcon}>{leftIcon}</View>}

          <TextInput
            ref={ref}
            style={[
              styles.input,
              leftIcon ? styles.inputWithLeftIcon : undefined,
              rightIcon ? styles.inputWithRightIcon : undefined,
              style,
            ]}
            placeholderTextColor={colors.textTertiary}
            {...props}
          />

          {rightIcon && <View style={styles.rightIcon}>{rightIcon}</View>}
        </View>

        {error && <Text style={styles.errorText}>{error}</Text>}
      </View>
    );
  }
);

Input.displayName = "Input";

const styles = StyleSheet.create({
  container: {
    marginBottom: spacing.lg,
  },
  label: {
    ...typography.styles.caption,
    fontWeight: "600",
    color: colors.textPrimary,
    marginBottom: spacing.sm,
  },
  inputContainer: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: colors.background,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: borderRadius.sm,
  },
  inputError: {
    borderColor: colors.error,
  },
  input: {
    flex: 1,
    height: 50,
    paddingHorizontal: spacing.lg,
    fontSize: typography.fontSize.lg,
    color: colors.textPrimary,
  },
  inputWithLeftIcon: {
    paddingLeft: spacing.sm,
  },
  inputWithRightIcon: {
    paddingRight: spacing.sm,
  },
  leftIcon: {
    paddingLeft: spacing.md,
  },
  rightIcon: {
    paddingRight: spacing.md,
  },
  errorText: {
    ...typography.styles.small,
    color: colors.error,
    marginTop: spacing.xs,
  },
});

export default Input;
