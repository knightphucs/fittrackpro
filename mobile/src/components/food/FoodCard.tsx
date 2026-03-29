// Food Card Component - Display food item in list
import React from "react";
import { View, Text, Image, StyleSheet } from "react-native";
import { Food } from "../../types";
import { colors, spacing, borderRadius, typography } from "../../constants";
import { config } from "../../constants";
import Card from "../common/Card";

interface FoodCardProps {
  food: Food;
  onPress?: () => void;
}

const FoodCard: React.FC<FoodCardProps> = ({ food, onPress }) => {
  // Prefer Vietnamese name
  const displayName = food.nameVi || food.name;

  return (
    <Card onPress={onPress} style={styles.card}>
      <View style={styles.content}>
        <Image
          source={{
            uri: food.imageUrl || config.images.placeholderUrl,
          }}
          style={styles.image}
        />

        <View style={styles.info}>
          <View style={styles.headerRow}>
            <Text style={styles.name} numberOfLines={1}>
              {displayName}
            </Text>
            <Text style={styles.calories}>{food.calories} kcal</Text>
          </View>

          <Text style={styles.serving}>
            {food.servingSize} {food.servingUnit}
          </Text>

          {/* Macros */}
          <View style={styles.macroContainer}>
            <MacroItem
              label="Pro"
              value={food.protein}
              color={colors.protein}
            />
            <View style={styles.divider} />
            <MacroItem label="Carb" value={food.carbs} color={colors.carbs} />
            <View style={styles.divider} />
            <MacroItem label="Fat" value={food.fat} color={colors.fat} />
          </View>
        </View>
      </View>
    </Card>
  );
};

// Macro item sub-component
interface MacroItemProps {
  label: string;
  value: number;
  color: string;
}

const MacroItem: React.FC<MacroItemProps> = ({ label, value, color }) => (
  <View style={styles.macroItem}>
    <Text style={[styles.macroLabel, { color }]}>{label}</Text>
    <Text style={styles.macroValue}>{value}g</Text>
  </View>
);

const styles = StyleSheet.create({
  card: {
    padding: 0,
  },
  content: {
    flexDirection: "row",
    padding: spacing.cardPadding,
  },
  image: {
    width: 80,
    height: 80,
    borderRadius: borderRadius.md,
    backgroundColor: colors.background,
  },
  info: {
    flex: 1,
    marginLeft: spacing.md,
    justifyContent: "center",
  },
  headerRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: spacing.xs,
  },
  name: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
    flex: 1,
    marginRight: spacing.sm,
  },
  calories: {
    ...typography.styles.caption,
    fontWeight: "bold",
    color: colors.calories,
  },
  serving: {
    ...typography.styles.small,
    color: colors.textSecondary,
    marginBottom: spacing.sm,
  },
  macroContainer: {
    flexDirection: "row",
    backgroundColor: colors.background,
    borderRadius: borderRadius.sm,
    padding: spacing.xs,
    justifyContent: "space-between",
  },
  macroItem: {
    alignItems: "center",
    flex: 1,
  },
  macroLabel: {
    ...typography.styles.tiny,
    fontWeight: "bold",
  },
  macroValue: {
    ...typography.styles.small,
    fontWeight: "600",
    color: colors.textPrimary,
  },
  divider: {
    width: 1,
    backgroundColor: colors.border,
    marginHorizontal: spacing.xs,
  },
});

export default FoodCard;
