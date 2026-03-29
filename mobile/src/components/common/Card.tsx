// Card Component
import React, { ReactNode } from "react";
import { View, StyleSheet, ViewStyle, TouchableOpacity } from "react-native";
import { colors, spacing, borderRadius } from "../../constants";

interface CardProps {
  children: ReactNode;
  style?: ViewStyle;
  onPress?: () => void;
  variant?: "default" | "elevated" | "outlined";
}

const Card: React.FC<CardProps> = ({
  children,
  style,
  onPress,
  variant = "elevated",
}) => {
  const cardStyles = [styles.base, styles[variant], style];

  if (onPress) {
    return (
      <TouchableOpacity
        style={cardStyles}
        onPress={onPress}
        activeOpacity={0.7}
      >
        {children}
      </TouchableOpacity>
    );
  }

  return <View style={cardStyles}>{children}</View>;
};

const styles = StyleSheet.create({
  base: {
    backgroundColor: colors.white,
    borderRadius: borderRadius.lg,
    padding: spacing.cardPadding,
    marginBottom: spacing.md,
  },
  default: {
    // No additional styles
  },
  elevated: {
    shadowColor: colors.black,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  outlined: {
    borderWidth: 1,
    borderColor: colors.border,
  },
});

export default Card;
