// Home Screen - Dashboard
import React, { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
} from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { MainTabScreenProps } from "../../navigation/types";
import { analyticsApi } from "../../api";
import { DashboardData } from "../../types";
import { colors, spacing, borderRadius, typography } from "../../constants";
import { Card, Loading } from "../../components";

const HomeScreen: React.FC = () => {
  const navigation = useNavigation<MainTabScreenProps<"Home">["navigation"]>();
  const [dashboard, setDashboard] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchDashboard = useCallback(async (isRefresh = false) => {
    if (!isRefresh) setLoading(true);
    try {
      const data = await analyticsApi.getDashboard();
      setDashboard(data);
    } catch (error) {
      console.error("Error fetching dashboard:", error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  const handleRefresh = () => {
    setRefreshing(true);
    fetchDashboard(true);
  };

  if (loading) {
    return <Loading fullScreen text="Đang tải..." />;
  }

  const caloriePercentage =
    dashboard && dashboard.targetCalories > 0
      ? Math.min(
          (dashboard.todayCalories / dashboard.targetCalories) * 100,
          100
        )
      : 0;

  return (
    <ScrollView
      style={styles.container}
      contentContainerStyle={styles.content}
      refreshControl={
        <RefreshControl
          refreshing={refreshing}
          onRefresh={handleRefresh}
          colors={[colors.primary]}
        />
      }
    >
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.greeting}>Xin chào! 👋</Text>
        <Text style={styles.date}>
          {new Date().toLocaleDateString("vi-VN", {
            weekday: "long",
            day: "numeric",
            month: "long",
          })}
        </Text>
      </View>

      {/* Calories Card */}
      <Card style={styles.caloriesCard}>
        <Text style={styles.cardTitle}>Calories hôm nay</Text>
        <View style={styles.caloriesContent}>
          <View style={styles.caloriesMain}>
            <Text style={styles.caloriesValue}>
              {dashboard?.todayCalories || 0}
            </Text>
            <Text style={styles.caloriesTarget}>
              / {dashboard?.targetCalories || 2000} kcal
            </Text>
          </View>
          <View style={styles.progressBarContainer}>
            <View
              style={[styles.progressBar, { width: `${caloriePercentage}%` }]}
            />
          </View>
          <Text style={styles.caloriesRemaining}>
            Còn lại: {dashboard?.caloriesRemaining || 0} kcal
          </Text>
        </View>
      </Card>

      {/* Macros Row */}
      <View style={styles.macrosRow}>
        <MacroCard
          label="Protein"
          value={dashboard?.todayProtein || 0}
          color={colors.protein}
        />
        <MacroCard
          label="Carbs"
          value={dashboard?.todayCarbs || 0}
          color={colors.carbs}
        />
        <MacroCard
          label="Fat"
          value={dashboard?.todayFat || 0}
          color={colors.fat}
        />
      </View>

      {/* Stats Row */}
      <View style={styles.statsRow}>
        <Card style={styles.statCard}>
          <Text style={styles.statIcon}>🔥</Text>
          <Text style={styles.statValue}>{dashboard?.streakDays || 0}</Text>
          <Text style={styles.statLabel}>Ngày liên tiếp</Text>
        </Card>
        <Card style={styles.statCard}>
          <Text style={styles.statIcon}>💪</Text>
          <Text style={styles.statValue}>
            {dashboard?.workoutsThisWeek || 0}
          </Text>
          <Text style={styles.statLabel}>Buổi tập tuần này</Text>
        </Card>
      </View>

      {/* Weight Progress */}
      {dashboard?.weightProgress && (
        <Card style={styles.weightCard}>
          <Text style={styles.cardTitle}>Tiến độ cân nặng</Text>
          <View style={styles.weightContent}>
            <View style={styles.weightItem}>
              <Text style={styles.weightLabel}>Hiện tại</Text>
              <Text style={styles.weightValue}>
                {dashboard.weightProgress.current} kg
              </Text>
            </View>
            <View
              style={[
                styles.weightArrow,
                dashboard.weightProgress.change > 0
                  ? styles.weightArrowPositive
                  : dashboard.weightProgress.change < 0
                  ? styles.weightArrowNegative
                  : styles.weightArrowNeutral,
              ]}
            >
              <Text style={styles.weightChange}>
                {dashboard.weightProgress.change > 0
                  ? "▲ +"
                  : dashboard.weightProgress.change < 0
                  ? "▼ "
                  : "= "}
                {Math.abs(dashboard.weightProgress.change).toFixed(2)} kg
              </Text>
            </View>
            <View style={styles.weightItem}>
              <Text style={styles.weightLabel}>Mục tiêu</Text>
              <Text style={styles.weightValue}>
                {dashboard.weightProgress.target} kg
              </Text>
            </View>
          </View>
        </Card>
      )}

      {/* Quick Actions */}
      <Text style={styles.sectionTitle}>Thao tác nhanh</Text>
      <View style={styles.actionsRow}>
        <TouchableOpacity
          style={styles.actionButton}
          onPress={() => navigation.navigate("Foods", { screen: "FoodList" })}
        >
          <Text style={styles.actionIcon}>🍽️</Text>
          <Text style={styles.actionLabel}>Thêm bữa ăn</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={styles.actionButton}
          onPress={() =>
            navigation.navigate("Workouts", { screen: "WorkoutList" })
          }
        >
          <Text style={styles.actionIcon}>🏋️</Text>
          <Text style={styles.actionLabel}>Bắt đầu tập</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={styles.actionButton}
          onPress={() =>
            navigation.navigate("Progress", { screen: "ProgressHistory" })
          }
        >
          <Text style={styles.actionIcon}>⚖️</Text>
          <Text style={styles.actionLabel}>Ghi cân nặng</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
};

// Macro Card Component
const MacroCard: React.FC<{ label: string; value: number; color: string }> = ({
  label,
  value,
  color,
}) => (
  <Card style={styles.macroCard}>
    <View style={[styles.macroDot, { backgroundColor: color }]} />
    <Text style={styles.macroValue}>{value}g</Text>
    <Text style={styles.macroLabel}>{label}</Text>
  </Card>
);

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  content: {
    padding: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  header: {
    marginBottom: spacing.xl,
    paddingTop: spacing.xl,
  },
  greeting: {
    ...typography.styles.title,
    color: colors.textPrimary,
  },
  date: {
    ...typography.styles.body,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  caloriesCard: {
    backgroundColor: colors.primary,
    marginBottom: spacing.lg,
  },
  cardTitle: {
    ...typography.styles.bodyBold,
    color: colors.white,
    marginBottom: spacing.md,
  },
  caloriesContent: {
    alignItems: "center",
  },
  caloriesMain: {
    flexDirection: "row",
    alignItems: "baseline",
  },
  caloriesValue: {
    fontSize: 48,
    fontWeight: "bold",
    color: colors.white,
  },
  caloriesTarget: {
    ...typography.styles.body,
    color: "rgba(255,255,255,0.8)",
    marginLeft: spacing.sm,
  },
  progressBarContainer: {
    width: "100%",
    height: 8,
    backgroundColor: "rgba(255,255,255,0.3)",
    borderRadius: borderRadius.full,
    marginVertical: spacing.md,
  },
  progressBar: {
    height: "100%",
    backgroundColor: colors.white,
    borderRadius: borderRadius.full,
  },
  caloriesRemaining: {
    ...typography.styles.caption,
    color: "rgba(255,255,255,0.8)",
  },
  macrosRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: spacing.lg,
  },
  macroCard: {
    flex: 1,
    marginHorizontal: spacing.xs,
    alignItems: "center",
    paddingVertical: spacing.md,
  },
  macroDot: {
    width: 12,
    height: 12,
    borderRadius: 6,
    marginBottom: spacing.xs,
  },
  macroValue: {
    ...typography.styles.subheading,
    color: colors.textPrimary,
  },
  macroLabel: {
    ...typography.styles.small,
    color: colors.textSecondary,
  },
  statsRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: spacing.lg,
  },
  statCard: {
    flex: 1,
    marginHorizontal: spacing.xs,
    alignItems: "center",
    paddingVertical: spacing.lg,
  },
  statIcon: {
    fontSize: 32,
    marginBottom: spacing.sm,
  },
  statValue: {
    ...typography.styles.heading,
    color: colors.textPrimary,
  },
  statLabel: {
    ...typography.styles.small,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  weightCard: {
    marginBottom: spacing.lg,
  },
  weightContent: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  weightItem: {
    alignItems: "center",
  },
  weightLabel: {
    ...typography.styles.small,
    color: colors.textSecondary,
  },
  weightValue: {
    ...typography.styles.subheading,
    color: colors.textPrimary,
    marginTop: spacing.xs,
  },
  weightArrow: {
    paddingHorizontal: spacing.md,
  },
  weightArrowPositive: {
    backgroundColor: colors.successLight,
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.md,
    borderRadius: borderRadius.sm,
  },
  weightArrowNegative: {
    backgroundColor: colors.errorLight,
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.md,
    borderRadius: borderRadius.sm,
  },
  weightArrowNeutral: {
    backgroundColor: "rgba(0,0,0,0.05)",
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.md,
    borderRadius: borderRadius.sm,
  },
  weightChange: {
    ...typography.styles.bodyBold,
    color: colors.success,
  },
  sectionTitle: {
    ...typography.styles.subheading,
    color: colors.textPrimary,
    marginBottom: spacing.md,
  },
  actionsRow: {
    flexDirection: "row",
    justifyContent: "space-between",
  },
  actionButton: {
    flex: 1,
    backgroundColor: colors.white,
    marginHorizontal: spacing.xs,
    paddingVertical: spacing.lg,
    borderRadius: borderRadius.lg,
    alignItems: "center",
    shadowColor: colors.black,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 2,
  },
  actionIcon: {
    fontSize: 28,
    marginBottom: spacing.sm,
  },
  actionLabel: {
    ...typography.styles.small,
    color: colors.textPrimary,
    fontWeight: "500",
  },
});

export default HomeScreen;
