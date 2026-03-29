// Nutrition Label Component - Display macro breakdown
import React from "react";
import { View, Text, StyleSheet } from "react-native";
import { colors, spacing, borderRadius, typography } from "../../constants";

interface NutritionLabelProps {
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  fiber?: number;
  sugar?: number;
  servingSize?: number;
  servingUnit?: string;
}

const NutritionLabel: React.FC<NutritionLabelProps> = ({
  calories,
  protein,
  carbs,
  fat,
  fiber,
  sugar,
  servingSize,
  servingUnit,
}) => {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Thông tin dinh dưỡng</Text>

      {servingSize && servingUnit && (
        <Text style={styles.serving}>
          Khẩu phần: {servingSize} {servingUnit}
        </Text>
      )}

      {/* Calories */}
      <View style={styles.caloriesRow}>
        <Text style={styles.caloriesLabel}>Năng lượng</Text>
        <Text style={styles.caloriesValue}>{calories} kcal</Text>
      </View>

      <View style={styles.divider} />

      {/* Main macros */}
      <NutritionRow
        label="Protein"
        value={protein}
        unit="g"
        color={colors.protein}
      />
      <NutritionRow
        label="Carbohydrate"
        value={carbs}
        unit="g"
        color={colors.carbs}
      />
      <NutritionRow label="Chất béo" value={fat} unit="g" color={colors.fat} />

      {/* Optional macros */}
      {fiber !== undefined && (
        <NutritionRow
          label="Chất xơ"
          value={fiber}
          unit="g"
          color={colors.fiber}
          indent
        />
      )}
      {sugar !== undefined && (
        <NutritionRow
          label="Đường"
          value={sugar}
          unit="g"
          color={colors.carbs}
          indent
        />
      )}
    </View>
  );
};

interface NutritionRowProps {
  label: string;
  value: number;
  unit: string;
  color?: string;
  indent?: boolean;
}

const NutritionRow: React.FC<NutritionRowProps> = ({
  label,
  value,
  unit,
  color,
  indent = false,
}) => (
  <View style={[styles.row, indent && styles.indentedRow]}>
    <View style={styles.labelContainer}>
      {color && <View style={[styles.colorDot, { backgroundColor: color }]} />}
      <Text style={[styles.label, indent && styles.indentedLabel]}>
        {label}
      </Text>
    </View>
    <Text style={styles.value}>
      {value} {unit}
    </Text>
  </View>
);

const styles = StyleSheet.create({
  container: {
    backgroundColor: colors.white,
    borderRadius: borderRadius.lg,
    padding: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
  },
  title: {
    ...typography.styles.subheading,
    color: colors.textPrimary,
    marginBottom: spacing.xs,
  },
  serving: {
    ...typography.styles.caption,
    color: colors.textSecondary,
    marginBottom: spacing.md,
  },
  caloriesRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingVertical: spacing.sm,
  },
  caloriesLabel: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
  },
  caloriesValue: {
    ...typography.styles.heading,
    color: colors.calories,
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.sm,
  },
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingVertical: spacing.sm,
  },
  indentedRow: {
    paddingLeft: spacing.lg,
  },
  labelContainer: {
    flexDirection: "row",
    alignItems: "center",
  },
  colorDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    marginRight: spacing.sm,
  },
  label: {
    ...typography.styles.body,
    color: colors.textPrimary,
  },
  indentedLabel: {
    ...typography.styles.caption,
    color: colors.textSecondary,
  },
  value: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
  },
});

export default NutritionLabel;
