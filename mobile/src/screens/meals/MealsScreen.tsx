// Meals Screen
import React, { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  Alert,
} from "react-native";
import { mealsApi } from "../../api";
import { DailyMeals, MealLog, MealType } from "../../types";
import { colors, spacing, borderRadius, typography } from "../../constants";
import { Card, Loading } from "../../components";

const MealsScreen: React.FC = () => {
  const [dailyMeals, setDailyMeals] = useState<DailyMeals | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [selectedDate, setSelectedDate] = useState(new Date());

  const fetchMeals = useCallback(
    async (isRefresh = false) => {
      if (!isRefresh) setLoading(true);
      try {
        // Format date as YYYY-MM-DD in local timezone
        const year = selectedDate.getFullYear();
        const month = String(selectedDate.getMonth() + 1).padStart(2, "0");
        const day = String(selectedDate.getDate()).padStart(2, "0");
        const dateString = `${year}-${month}-${day}`;
        const data = await mealsApi.getDailyMeals(dateString);
        setDailyMeals(data);
      } catch (error) {
        if (error && typeof error === "object" && "response" in error) {
          const resp = (error as any).response;
          Alert.alert("Lỗi", resp.data?.message || "Không thể tải dữ liệu");
        }
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [selectedDate]
  );

  useEffect(() => {
    fetchMeals();
  }, [fetchMeals]);

  const handleDeleteMeal = async (logId: string) => {
    Alert.alert("Xóa", "Bạn có chắc muốn xóa món này?", [
      { text: "Hủy", style: "cancel" },
      {
        text: "Xóa",
        style: "destructive",
        onPress: async () => {
          try {
            await mealsApi.deleteMealLog(logId);
            fetchMeals(true);
          } catch (error) {
            Alert.alert("Lỗi", "Không thể xóa");
          }
        },
      },
    ]);
  };

  const changeDate = (days: number) => {
    const newDate = new Date(selectedDate);
    newDate.setDate(newDate.getDate() + days);
    setSelectedDate(newDate);
  };

  if (loading) {
    return <Loading fullScreen text="Đang tải..." />;
  }

  const mealTypes: { type: MealType; label: string; icon: string }[] = [
    { type: "Breakfast", label: "Bữa sáng", icon: "🌅" },
    { type: "Lunch", label: "Bữa trưa", icon: "☀️" },
    { type: "Dinner", label: "Bữa tối", icon: "🌙" },
    { type: "Snack", label: "Bữa phụ", icon: "🍎" },
  ];

  const getMealsByType = (type: MealType): MealLog[] => {
    return dailyMeals?.meals?.filter((meal) => meal.mealType === type) || [];
  };

  const getTotalByType = (type: MealType) => {
    const meals = getMealsByType(type);
    return meals.reduce((sum, meal) => sum + meal.totalCalories, 0);
  };

  return (
    <View style={styles.container}>
      {/* Header with Date Selector */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Bữa ăn</Text>
        <View style={styles.dateSelector}>
          <TouchableOpacity onPress={() => changeDate(-1)}>
            <Text style={styles.dateArrow}>‹</Text>
          </TouchableOpacity>
          <Text style={styles.dateText}>
            {selectedDate.toLocaleDateString("vi-VN", {
              weekday: "short",
              day: "numeric",
              month: "short",
            })}
          </Text>
          <TouchableOpacity onPress={() => changeDate(1)}>
            <Text style={styles.dateArrow}>›</Text>
          </TouchableOpacity>
        </View>
      </View>

      {/* Daily Summary */}
      {dailyMeals?.summary && (
        <Card style={styles.summaryCard}>
          <View style={styles.summaryMain}>
            <Text style={styles.summaryCalories}>
              {dailyMeals.summary.totalCalories}
            </Text>
            <Text style={styles.summaryLabel}>
              / {dailyMeals.summary.targetCalories} kcal
            </Text>
          </View>
          <View style={styles.summaryMacros}>
            <MacroBadge
              label="P"
              value={dailyMeals.summary.totalProtein}
              color={colors.protein}
            />
            <MacroBadge
              label="C"
              value={dailyMeals.summary.totalCarbs}
              color={colors.carbs}
            />
            <MacroBadge
              label="F"
              value={dailyMeals.summary.totalFat}
              color={colors.fat}
            />
          </View>
        </Card>
      )}

      {/* Meals List */}
      <ScrollView
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => {
              setRefreshing(true);
              fetchMeals(true);
            }}
            colors={[colors.primary]}
          />
        }
      >
        {mealTypes.map(({ type, label, icon }) => (
          <MealSection
            key={type}
            icon={icon}
            label={label}
            totalCalories={getTotalByType(type)}
            meals={getMealsByType(type)}
            onDelete={handleDeleteMeal}
          />
        ))}
      </ScrollView>
    </View>
  );
};

// Macro Badge Component
const MacroBadge: React.FC<{ label: string; value: number; color: string }> = ({
  label,
  value,
  color,
}) => (
  <View style={styles.macroBadge}>
    <View style={[styles.macroDot, { backgroundColor: color }]} />
    <Text style={styles.macroBadgeText}>
      {label}: {value}g
    </Text>
  </View>
);

// Meal Section Component
const MealSection: React.FC<{
  icon: string;
  label: string;
  totalCalories: number;
  meals: MealLog[];
  onDelete: (id: string) => void;
}> = ({ icon, label, totalCalories, meals, onDelete }) => (
  <Card style={styles.mealSection}>
    <View style={styles.mealSectionHeader}>
      <View style={styles.mealSectionLeft}>
        <Text style={styles.mealSectionIcon}>{icon}</Text>
        <Text style={styles.mealSectionLabel}>{label}</Text>
      </View>
      <Text style={styles.mealSectionCalories}>{totalCalories} kcal</Text>
    </View>
    {meals.length > 0 ? (
      meals.map((meal) => (
        <TouchableOpacity
          key={meal.id}
          style={styles.mealItem}
          onLongPress={() => onDelete(meal.id)}
        >
          <View style={styles.mealItemInfo}>
            <Text style={styles.mealItemName}>{meal.foodName}</Text>
            <Text style={styles.mealItemServing}>
              {meal.servingMultiplier} phần
            </Text>
          </View>
          <Text style={styles.mealItemCalories}>{meal.totalCalories} kcal</Text>
        </TouchableOpacity>
      ))
    ) : (
      <View style={styles.mealEmptyContainer}>
        <Text style={styles.mealEmptyText}>Chưa có gì</Text>
      </View>
    )}
    <TouchableOpacity style={styles.addMealButton}>
      <Text style={styles.addMealText}>+ Thêm món</Text>
    </TouchableOpacity>
  </Card>
);

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    backgroundColor: colors.white,
    paddingTop: 50,
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.md,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  headerTitle: {
    ...typography.styles.title,
    color: colors.textPrimary,
  },
  dateSelector: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: colors.background,
    borderRadius: borderRadius.md,
    paddingHorizontal: spacing.sm,
    paddingVertical: spacing.xs,
  },
  dateArrow: {
    fontSize: 24,
    color: colors.primary,
    paddingHorizontal: spacing.sm,
  },
  dateText: {
    ...typography.styles.body,
    color: colors.textPrimary,
    fontWeight: "500",
    paddingHorizontal: spacing.sm,
  },
  summaryCard: {
    marginHorizontal: spacing.lg,
    marginTop: spacing.md,
    backgroundColor: colors.primary,
    padding: spacing.lg,
  },
  summaryMain: {
    flexDirection: "row",
    alignItems: "baseline",
    marginBottom: spacing.md,
  },
  summaryCalories: {
    fontSize: 36,
    fontWeight: "bold",
    color: colors.white,
  },
  summaryLabel: {
    ...typography.styles.body,
    color: "rgba(255,255,255,0.8)",
    marginLeft: spacing.sm,
  },
  summaryMacros: {
    flexDirection: "row",
  },
  macroBadge: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "rgba(255,255,255,0.2)",
    paddingHorizontal: spacing.sm,
    paddingVertical: spacing.xs,
    borderRadius: borderRadius.sm,
    marginRight: spacing.sm,
  },
  macroDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    marginRight: spacing.xs,
  },
  macroBadgeText: {
    ...typography.styles.small,
    color: colors.white,
    fontWeight: "500",
  },
  content: {
    padding: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  mealSection: {
    marginBottom: spacing.md,
    padding: 0,
    overflow: "hidden",
  },
  mealSectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: spacing.md,
    backgroundColor: colors.backgroundSecondary,
  },
  mealSectionLeft: {
    flexDirection: "row",
    alignItems: "center",
  },
  mealSectionIcon: {
    fontSize: 20,
    marginRight: spacing.sm,
  },
  mealSectionLabel: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
  },
  mealSectionCalories: {
    ...typography.styles.body,
    color: colors.primary,
    fontWeight: "600",
  },
  mealItem: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  mealItemInfo: {
    flex: 1,
  },
  mealItemName: {
    ...typography.styles.body,
    color: colors.textPrimary,
  },
  mealItemServing: {
    ...typography.styles.small,
    color: colors.textSecondary,
    marginTop: 2,
  },
  mealItemCalories: {
    ...typography.styles.body,
    color: colors.textSecondary,
  },
  mealEmptyContainer: {
    padding: spacing.lg,
    alignItems: "center",
  },
  mealEmptyText: {
    ...typography.styles.caption,
    color: colors.textTertiary,
  },
  addMealButton: {
    padding: spacing.md,
    alignItems: "center",
    borderTopWidth: 1,
    borderTopColor: colors.borderLight,
  },
  addMealText: {
    ...typography.styles.body,
    color: colors.primary,
    fontWeight: "600",
  },
});

export default MealsScreen;
