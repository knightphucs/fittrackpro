// Progress Screen
import React, { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  Dimensions,
} from "react-native";
import { useFocusEffect } from "@react-navigation/native";
import { progressApi } from "../../api";
import { ProgressEntry, ProgressPhoto, ProgressStatistics } from "../../types";
import { colors, spacing, borderRadius, typography } from "../../constants";
import { Card, Loading, Button } from "../../components";

const { width } = Dimensions.get("window");

const ProgressScreen: React.FC = () => {
  const [weightLogs, setWeightLogs] = useState<ProgressEntry[]>([]);
  const [photos, setPhotos] = useState<ProgressPhoto[]>([]);
  const [statistics, setStatistics] = useState<ProgressStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [activeTab, setActiveTab] = useState<"weight" | "photos">("weight");

  const fetchData = useCallback(async (isRefresh = false) => {
    if (!isRefresh) setLoading(true);
    try {
      // Fetch data with error handling for empty states
      const [statsResult, weightResult, photosResult] =
        await Promise.allSettled([
          progressApi.getStatistics(),
          progressApi.getHistory({}),
          progressApi.getPhotos({}),
        ]);

      // Handle statistics (404 if no data is OK)
      if (statsResult.status === "fulfilled") {
        setStatistics(statsResult.value);
      } else if (statsResult.reason?.response?.status === 404) {
        setStatistics(null);
      } else {
        console.error("❌ Error fetching statistics:", statsResult.reason);
      }

      // Handle weight history (404 if no data is OK)
      if (weightResult.status === "fulfilled") {
        const historyData = weightResult.value;
        setWeightLogs(Array.isArray(historyData) ? historyData : []);
      } else if (weightResult.reason?.response?.status === 404) {
        setWeightLogs([]);
      } else {
        console.error("❌ Error fetching weight history:", weightResult.reason);
        setWeightLogs([]);
      }

      // Handle photos (404 if no data is OK)
      if (photosResult.status === "fulfilled") {
        setPhotos(photosResult.value);
      } else if (photosResult.reason?.response?.status === 404) {
        setPhotos([]);
      } else {
        console.error("❌ Error fetching photos:", photosResult.reason);
      }
    } catch (error) {
      console.error("❌ Unexpected error fetching progress:", error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Refresh when screen is focused
  useFocusEffect(
    useCallback(() => {
      fetchData();
    }, [fetchData])
  );

  if (loading) {
    return <Loading fullScreen text="Đang tải..." />;
  }

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Tiến trình</Text>
        <TouchableOpacity style={styles.addButton}>
          <Text style={styles.addButtonText}>+ Ghi cân</Text>
        </TouchableOpacity>
      </View>

      {/* Statistics */}
      {statistics && (
        <Card style={styles.statsCard}>
          <View style={styles.statsRow}>
            <View style={styles.statItem}>
              <Text style={styles.statValue}>{statistics.startWeight}</Text>
              <Text style={styles.statLabel}>kg bắt đầu</Text>
            </View>
            <View style={styles.statDivider} />
            <View style={styles.statItem}>
              <Text style={[styles.statValue, styles.statCurrent]}>
                {statistics.currentWeight}
              </Text>
              <Text style={styles.statLabel}>kg hiện tại</Text>
            </View>
            <View style={styles.statDivider} />
            <View style={styles.statItem}>
              <Text
                style={[
                  styles.statValue,
                  statistics.totalWeightChange < 0
                    ? styles.statPositive
                    : styles.statNegative,
                ]}
              >
                {statistics.totalWeightChange > 0 ? "+" : ""}
                {statistics.totalWeightChange}
              </Text>
              <Text style={styles.statLabel}>kg thay đổi</Text>
            </View>
          </View>
        </Card>
      )}

      {/* Tabs */}
      <View style={styles.tabs}>
        <TouchableOpacity
          style={[styles.tab, activeTab === "weight" && styles.tabActive]}
          onPress={() => setActiveTab("weight")}
        >
          <Text
            style={[
              styles.tabText,
              activeTab === "weight" && styles.tabTextActive,
            ]}
          >
            ⚖️ Cân nặng
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === "photos" && styles.tabActive]}
          onPress={() => setActiveTab("photos")}
        >
          <Text
            style={[
              styles.tabText,
              activeTab === "photos" && styles.tabTextActive,
            ]}
          >
            📷 Ảnh
          </Text>
        </TouchableOpacity>
      </View>

      {/* Content */}
      <ScrollView
        contentContainerStyle={styles.content}
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
      >
        {activeTab === "weight" ? (
          <>
            {/* Weight Chart Placeholder */}
            <Card style={styles.chartCard}>
              <Text style={styles.chartTitle}>Biểu đồ cân nặng</Text>
              <View style={styles.chartPlaceholder}>
                <Text style={styles.chartPlaceholderText}>📈</Text>
                <Text style={styles.chartPlaceholderSubtext}>
                  Biểu đồ sẽ hiển thị ở đây
                </Text>
              </View>
            </Card>

            {/* Weight History */}
            <Text style={styles.sectionTitle}>
              Lịch sử cân nặng ({weightLogs.length} mục)
            </Text>
            {weightLogs && weightLogs.length > 0 ? (
              weightLogs.map((log, index) => (
                <Card key={log.id} style={styles.weightCard}>
                  <View style={styles.weightCardContent}>
                    <View>
                      <Text style={styles.weightValue}>{log.weight} kg</Text>
                      <Text style={styles.weightDate}>
                        {new Date(log.recordedAt).toLocaleDateString("vi-VN", {
                          weekday: "short",
                          day: "numeric",
                          month: "short",
                          year: "numeric",
                        })}
                      </Text>
                    </View>
                    {index < weightLogs.length - 1 && (
                      <View
                        style={[
                          styles.weightChange,
                          log.weight > weightLogs[index + 1]?.weight
                            ? styles.weightChangePositive
                            : styles.weightChangeNegative,
                        ]}
                      >
                        <Text style={styles.weightChangeText}>
                          {log.weight > weightLogs[index + 1]?.weight
                            ? "▲"
                            : "▼"}{" "}
                          {Math.abs(
                            log.weight - (weightLogs[index + 1]?.weight || 0)
                          ).toFixed(2)}{" "}
                          kg
                        </Text>
                      </View>
                    )}
                  </View>
                </Card>
              ))
            ) : (
              <View style={styles.emptyContainer}>
                <Text style={styles.emptyIcon}>⚖️</Text>
                <Text style={styles.emptyText}>Chưa có dữ liệu cân nặng</Text>
              </View>
            )}
          </>
        ) : (
          <>
            {/* Photos Grid */}
            {photos.length > 0 ? (
              <View style={styles.photosGrid}>
                {photos.map((photo) => (
                  <TouchableOpacity key={photo.id} style={styles.photoItem}>
                    <View style={styles.photoPlaceholder}>
                      <Text style={styles.photoIcon}>📷</Text>
                    </View>
                    <Text style={styles.photoDate}>
                      {new Date(photo.takenAt).toLocaleDateString("vi-VN")}
                    </Text>
                    {photo.photoType && (
                      <Text style={styles.photoCategory}>
                        {photo.photoType}
                      </Text>
                    )}
                  </TouchableOpacity>
                ))}
              </View>
            ) : (
              <View style={styles.emptyContainer}>
                <Text style={styles.emptyIcon}>📷</Text>
                <Text style={styles.emptyText}>Chưa có ảnh tiến trình</Text>
                <Button
                  title="Thêm ảnh đầu tiên"
                  onPress={() => {}}
                  variant="outline"
                  style={styles.emptyButton}
                />
              </View>
            )}
          </>
        )}
      </ScrollView>
    </View>
  );
};

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
  addButton: {
    backgroundColor: colors.primary,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
    borderRadius: borderRadius.md,
  },
  addButtonText: {
    ...typography.styles.caption,
    color: colors.white,
    fontWeight: "600",
  },
  statsCard: {
    marginHorizontal: spacing.lg,
    marginTop: spacing.md,
    backgroundColor: colors.primary,
    padding: spacing.lg,
  },
  statsRow: {
    flexDirection: "row",
    justifyContent: "space-around",
    alignItems: "center",
  },
  statItem: {
    alignItems: "center",
  },
  statDivider: {
    width: 1,
    height: 40,
    backgroundColor: "rgba(255,255,255,0.3)",
  },
  statValue: {
    ...typography.styles.heading,
    color: colors.white,
  },
  statCurrent: {
    fontSize: 28,
  },
  statPositive: {
    color: "#4ADE80",
  },
  statNegative: {
    color: "#FB7185",
  },
  statLabel: {
    ...typography.styles.small,
    color: "rgba(255,255,255,0.8)",
    marginTop: spacing.xs,
  },
  tabs: {
    flexDirection: "row",
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.md,
  },
  tab: {
    flex: 1,
    paddingVertical: spacing.md,
    alignItems: "center",
    borderRadius: borderRadius.md,
    marginHorizontal: spacing.xs,
    backgroundColor: colors.white,
  },
  tabActive: {
    backgroundColor: colors.primaryLight,
  },
  tabText: {
    ...typography.styles.body,
    color: colors.textSecondary,
  },
  tabTextActive: {
    color: colors.primary,
    fontWeight: "600",
  },
  content: {
    padding: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  chartCard: {
    marginBottom: spacing.lg,
  },
  chartTitle: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
    marginBottom: spacing.md,
  },
  chartPlaceholder: {
    height: 200,
    backgroundColor: colors.background,
    borderRadius: borderRadius.md,
    justifyContent: "center",
    alignItems: "center",
  },
  chartPlaceholderText: {
    fontSize: 40,
    marginBottom: spacing.sm,
  },
  chartPlaceholderSubtext: {
    ...typography.styles.caption,
    color: colors.textTertiary,
  },
  sectionTitle: {
    ...typography.styles.subheading,
    color: colors.textPrimary,
    marginBottom: spacing.md,
  },
  weightCard: {
    marginBottom: spacing.sm,
  },
  weightCardContent: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  weightValue: {
    ...typography.styles.subheading,
    color: colors.textPrimary,
  },
  weightDate: {
    ...typography.styles.small,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  weightChange: {
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.xs,
    borderRadius: borderRadius.sm,
  },
  weightChangePositive: {
    backgroundColor: colors.successLight,
  },
  weightChangeNegative: {
    backgroundColor: colors.errorLight,
  },
  weightChangeText: {
    ...typography.styles.caption,
    fontWeight: "600",
  },
  photosGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    marginHorizontal: -spacing.xs,
  },
  photoItem: {
    width: (width - spacing.lg * 2 - spacing.xs * 4) / 2,
    marginHorizontal: spacing.xs,
    marginBottom: spacing.md,
  },
  photoPlaceholder: {
    aspectRatio: 3 / 4,
    backgroundColor: colors.border,
    borderRadius: borderRadius.md,
    justifyContent: "center",
    alignItems: "center",
  },
  photoIcon: {
    fontSize: 40,
  },
  photoDate: {
    ...typography.styles.small,
    color: colors.textPrimary,
    marginTop: spacing.xs,
  },
  photoCategory: {
    ...typography.styles.tiny,
    color: colors.textTertiary,
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
    color: colors.textSecondary,
  },
  emptyButton: {
    marginTop: spacing.lg,
  },
});

export default ProgressScreen;
