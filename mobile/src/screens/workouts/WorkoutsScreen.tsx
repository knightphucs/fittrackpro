// Workouts Screen
import React, { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  RefreshControl,
} from "react-native";
import { workoutsApi } from "../../api";
import { WorkoutSession, WorkoutSummary } from "../../types";
import { colors, spacing, borderRadius, typography } from "../../constants";
import { Card, Loading, Button } from "../../components";

const WorkoutsScreen: React.FC = () => {
  const [workouts, setWorkouts] = useState<WorkoutSession[]>([]);
  const [summary, setSummary] = useState<WorkoutSummary | null>(null);
  const [activeWorkout, setActiveWorkout] = useState<WorkoutSession | null>(
    null
  );
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchData = useCallback(async (isRefresh = false) => {
    if (!isRefresh) setLoading(true);
    try {
      const [historyData, summaryData, activeData] = await Promise.all([
        workoutsApi.getWorkoutHistory({ pageNumber: 1, pageSize: 10 }),
        workoutsApi.getWorkoutSummary(),
        workoutsApi.getActiveWorkout(),
      ]);
      setWorkouts(historyData.items);
      setSummary(summaryData);
      setActiveWorkout(activeData);
    } catch (error) {
      console.error("Error fetching workouts:", error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleStartWorkout = async () => {
    try {
      const workout = await workoutsApi.startWorkout({
        title: `Buổi tập ${new Date().toLocaleDateString("vi-VN")}`,
      });
      setActiveWorkout(workout);
    } catch (error) {
      console.error("Error starting workout:", error);
    }
  };

  const renderWorkoutItem = ({ item }: { item: WorkoutSession }) => (
    <Card style={styles.workoutCard}>
      <View style={styles.workoutHeader}>
        <View>
          <Text style={styles.workoutTitle}>{item.title}</Text>
          <Text style={styles.workoutDate}>
            {new Date(item.startedAt).toLocaleDateString("vi-VN", {
              weekday: "short",
              day: "numeric",
              month: "short",
            })}
          </Text>
        </View>
        <View
          style={[
            styles.statusBadge,
            item.status === "Completed" && styles.statusCompleted,
          ]}
        >
          <Text style={styles.statusText}>
            {item.status === "Completed" ? "Hoàn thành" : item.status}
          </Text>
        </View>
      </View>
      <View style={styles.workoutStats}>
        <StatItem icon="⏱️" value={`${item.durationMinutes} phút`} />
        <StatItem icon="🔥" value={`${item.totalCaloriesBurned} kcal`} />
        <StatItem icon="💪" value={`${item.exercises.length} bài tập`} />
      </View>
    </Card>
  );

  if (loading) {
    return <Loading fullScreen text="Đang tải..." />;
  }

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Tập luyện</Text>
      </View>

      {/* Active Workout Banner */}
      {activeWorkout && (
        <TouchableOpacity style={styles.activeBanner}>
          <View style={styles.activeDot} />
          <Text style={styles.activeText}>Đang tập: {activeWorkout.title}</Text>
          <Text style={styles.activeArrow}>›</Text>
        </TouchableOpacity>
      )}

      {/* Summary Stats */}
      {summary && (
        <View style={styles.summaryRow}>
          <Card style={styles.summaryCard}>
            <Text style={styles.summaryValue}>{summary.totalWorkouts}</Text>
            <Text style={styles.summaryLabel}>Tổng buổi tập</Text>
          </Card>
          <Card style={styles.summaryCard}>
            <Text style={styles.summaryValue}>{summary.workoutsThisWeek}</Text>
            <Text style={styles.summaryLabel}>Tuần này</Text>
          </Card>
          <Card style={styles.summaryCard}>
            <Text style={styles.summaryValue}>
              {summary.avgWorkoutDuration}
            </Text>
            <Text style={styles.summaryLabel}>Phút TB</Text>
          </Card>
        </View>
      )}

      {/* Start Workout Button */}
      {!activeWorkout && (
        <Button
          title="🏋️ Bắt đầu tập luyện"
          onPress={handleStartWorkout}
          style={styles.startButton}
          size="large"
        />
      )}

      {/* Workout History */}
      <Text style={styles.sectionTitle}>Lịch sử tập luyện</Text>
      <FlatList
        data={workouts}
        keyExtractor={(item) => item.id}
        renderItem={renderWorkoutItem}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => {
              setRefreshing(true);
              fetchData(true);
            }}
            colors={[colors.primary]}
          />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyIcon}>🏋️</Text>
            <Text style={styles.emptyText}>Chưa có buổi tập nào</Text>
            <Text style={styles.emptySubtext}>
              Bắt đầu tập luyện để theo dõi tiến trình
            </Text>
          </View>
        }
      />
    </View>
  );
};

// Stat Item Component
const StatItem: React.FC<{ icon: string; value: string }> = ({
  icon,
  value,
}) => (
  <View style={styles.statItem}>
    <Text style={styles.statIcon}>{icon}</Text>
    <Text style={styles.statValue}>{value}</Text>
  </View>
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
  },
  headerTitle: {
    ...typography.styles.title,
    color: colors.textPrimary,
  },
  activeBanner: {
    backgroundColor: colors.success,
    flexDirection: "row",
    alignItems: "center",
    padding: spacing.md,
    marginHorizontal: spacing.lg,
    marginTop: spacing.md,
    borderRadius: borderRadius.md,
  },
  activeDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: colors.white,
    marginRight: spacing.sm,
  },
  activeText: {
    ...typography.styles.bodyBold,
    color: colors.white,
    flex: 1,
  },
  activeArrow: {
    fontSize: 20,
    color: colors.white,
  },
  summaryRow: {
    flexDirection: "row",
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.md,
  },
  summaryCard: {
    flex: 1,
    marginHorizontal: spacing.xs,
    alignItems: "center",
    paddingVertical: spacing.md,
  },
  summaryValue: {
    ...typography.styles.heading,
    color: colors.primary,
  },
  summaryLabel: {
    ...typography.styles.small,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  startButton: {
    marginHorizontal: spacing.lg,
    marginBottom: spacing.lg,
  },
  sectionTitle: {
    ...typography.styles.subheading,
    color: colors.textPrimary,
    paddingHorizontal: spacing.lg,
    marginBottom: spacing.md,
  },
  list: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  workoutCard: {
    marginBottom: spacing.md,
  },
  workoutHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "flex-start",
    marginBottom: spacing.md,
  },
  workoutTitle: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
  },
  workoutDate: {
    ...typography.styles.small,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  statusBadge: {
    backgroundColor: colors.warning,
    paddingHorizontal: spacing.sm,
    paddingVertical: spacing.xs,
    borderRadius: borderRadius.sm,
  },
  statusCompleted: {
    backgroundColor: colors.success,
  },
  statusText: {
    ...typography.styles.tiny,
    color: colors.white,
    fontWeight: "600",
  },
  workoutStats: {
    flexDirection: "row",
    justifyContent: "space-around",
    paddingTop: spacing.md,
    borderTopWidth: 1,
    borderTopColor: colors.borderLight,
  },
  statItem: {
    flexDirection: "row",
    alignItems: "center",
  },
  statIcon: {
    fontSize: 16,
    marginRight: spacing.xs,
  },
  statValue: {
    ...typography.styles.caption,
    color: colors.textSecondary,
  },
  emptyContainer: {
    alignItems: "center",
    paddingTop: spacing.xxxl,
  },
  emptyIcon: {
    fontSize: 48,
    marginBottom: spacing.md,
  },
  emptyText: {
    ...typography.styles.body,
    color: colors.textPrimary,
    fontWeight: "600",
  },
  emptySubtext: {
    ...typography.styles.caption,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
});

export default WorkoutsScreen;
