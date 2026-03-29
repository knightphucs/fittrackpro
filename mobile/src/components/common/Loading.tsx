// Loading Component
import React from "react";
import {
  View,
  ActivityIndicator,
  Text,
  StyleSheet,
  ViewStyle,
} from "react-native";
import { colors, spacing, typography } from "../../constants";

interface LoadingProps {
  size?: "small" | "large";
  color?: string;
  text?: string;
  fullScreen?: boolean;
  style?: ViewStyle;
}

const Loading: React.FC<LoadingProps> = ({
  size = "large",
  color = colors.primary,
  text,
  fullScreen = false,
  style,
}) => {
  const containerStyle = fullScreen ? styles.fullScreen : styles.inline;

  return (
    <View style={[containerStyle, style]}>
      <ActivityIndicator size={size} color={color} />
      {text && <Text style={styles.text}>{text}</Text>}
    </View>
  );
};

const styles = StyleSheet.create({
  fullScreen: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: colors.background,
  },
  inline: {
    justifyContent: "center",
    alignItems: "center",
    padding: spacing.xl,
  },
  text: {
    marginTop: spacing.md,
    color: colors.textSecondary,
    fontSize: typography.fontSize.md,
  },
});

export default Loading;
