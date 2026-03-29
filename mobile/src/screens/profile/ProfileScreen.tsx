// Profile Screen
import React, { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
  Image,
  Alert,
} from "react-native";
import { usersApi } from "../../api";
import { useAuth } from "../../contexts";
import { UserProfile } from "../../types";
import {
  colors,
  spacing,
  borderRadius,
  typography,
  config,
} from "../../constants";
import { Card, Loading, Button } from "../../components";

const ProfileScreen: React.FC = () => {
  const { logout, user } = useAuth();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchProfile = useCallback(async (isRefresh = false) => {
    if (!isRefresh) setLoading(true);
    try {
      const data = await usersApi.getProfile();
      setProfile(data);
    } catch (error) {
      console.error("Error fetching profile:", error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  const handleLogout = () => {
    Alert.alert("Đăng xuất", "Bạn có chắc chắn muốn đăng xuất?", [
      { text: "Hủy", style: "cancel" },
      {
        text: "Đăng xuất",
        style: "destructive",
        onPress: logout,
      },
    ]);
  };

  if (loading) {
    return <Loading fullScreen text="Đang tải..." />;
  }

  return (
    <ScrollView
      style={styles.container}
      contentContainerStyle={styles.content}
      refreshControl={
        <RefreshControl
          refreshing={refreshing}
          onRefresh={() => {
            setRefreshing(true);
            fetchProfile(true);
          }}
          colors={[colors.primary]}
        />
      }
    >
      {/* Header */}
      <View style={styles.header}>
        <View style={styles.avatarContainer}>
          <Image
            source={{
              uri: profile?.profileImageUrl || config.images.placeholderUrl,
            }}
            style={styles.avatar}
          />
          <TouchableOpacity style={styles.editAvatarButton}>
            <Text style={styles.editAvatarIcon}>📷</Text>
          </TouchableOpacity>
        </View>
        <Text style={styles.name}>
          {profile?.firstName} {profile?.lastName}
        </Text>
        <Text style={styles.email}>{profile?.email}</Text>
      </View>

      {/* Stats */}
      <View style={styles.statsRow}>
        <View style={styles.statItem}>
          <Text style={styles.statValue}>{profile?.height || "-"}</Text>
          <Text style={styles.statLabel}>cm</Text>
        </View>
        <View style={styles.statDivider} />
        <View style={styles.statItem}>
          <Text style={styles.statValue}>{profile?.currentWeight || "-"}</Text>
          <Text style={styles.statLabel}>kg</Text>
        </View>
        <View style={styles.statDivider} />
        <View style={styles.statItem}>
          <Text style={styles.statValue}>{profile?.targetWeight || "-"}</Text>
          <Text style={styles.statLabel}>Mục tiêu</Text>
        </View>
      </View>

      {/* Menu Items */}
      <Card style={styles.menuCard}>
        <MenuItem icon="👤" label="Chỉnh sửa hồ sơ" onPress={() => {}} />
        <MenuItem icon="🎯" label="Mục tiêu" onPress={() => {}} />
        <MenuItem icon="📊" label="Báo cáo tuần" onPress={() => {}} />
        <MenuItem icon="🔔" label="Thông báo" onPress={() => {}} />
        <MenuItem
          icon="⚙️"
          label="Cài đặt"
          onPress={() => {}}
          showBorder={false}
        />
      </Card>

      {/* Info Card */}
      <Card style={styles.infoCard}>
        <Text style={styles.infoTitle}>Thông tin cá nhân</Text>
        <InfoRow label="Giới tính" value={profile?.gender || "Chưa cập nhật"} />
        <InfoRow
          label="Ngày sinh"
          value={
            profile?.dateOfBirth
              ? new Date(profile.dateOfBirth).toLocaleDateString("vi-VN")
              : "Chưa cập nhật"
          }
        />
        <InfoRow
          label="Mức độ vận động"
          value={profile?.activityLevel || "Chưa cập nhật"}
        />
        <InfoRow
          label="Mục tiêu cân nặng"
          value={profile?.weightGoal || "Chưa cập nhật"}
        />
        <InfoRow
          label="Calories mục tiêu"
          value={
            profile?.dailyCalorieTarget
              ? `${profile.dailyCalorieTarget} kcal`
              : "Chưa cập nhật"
          }
        />
      </Card>

      {/* Logout Button */}
      <Button
        title="Đăng xuất"
        onPress={handleLogout}
        variant="outline"
        style={styles.logoutButton}
      />
    </ScrollView>
  );
};

// Menu Item Component
const MenuItem: React.FC<{
  icon: string;
  label: string;
  onPress: () => void;
  showBorder?: boolean;
}> = ({ icon, label, onPress, showBorder = true }) => (
  <TouchableOpacity
    style={[styles.menuItem, showBorder && styles.menuItemBorder]}
    onPress={onPress}
  >
    <Text style={styles.menuIcon}>{icon}</Text>
    <Text style={styles.menuLabel}>{label}</Text>
    <Text style={styles.menuArrow}>›</Text>
  </TouchableOpacity>
);

// Info Row Component
const InfoRow: React.FC<{ label: string; value: string }> = ({
  label,
  value,
}) => (
  <View style={styles.infoRow}>
    <Text style={styles.infoLabel}>{label}</Text>
    <Text style={styles.infoValue}>{value}</Text>
  </View>
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
    alignItems: "center",
    paddingTop: spacing.xxl,
    paddingBottom: spacing.xl,
  },
  avatarContainer: {
    position: "relative",
    marginBottom: spacing.md,
  },
  avatar: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: colors.border,
  },
  editAvatarButton: {
    position: "absolute",
    bottom: 0,
    right: 0,
    backgroundColor: colors.primary,
    width: 32,
    height: 32,
    borderRadius: 16,
    justifyContent: "center",
    alignItems: "center",
    borderWidth: 2,
    borderColor: colors.white,
  },
  editAvatarIcon: {
    fontSize: 14,
  },
  name: {
    ...typography.styles.heading,
    color: colors.textPrimary,
  },
  email: {
    ...typography.styles.body,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  statsRow: {
    flexDirection: "row",
    backgroundColor: colors.white,
    borderRadius: borderRadius.lg,
    padding: spacing.lg,
    marginBottom: spacing.lg,
    shadowColor: colors.black,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 2,
  },
  statItem: {
    flex: 1,
    alignItems: "center",
  },
  statDivider: {
    width: 1,
    backgroundColor: colors.border,
  },
  statValue: {
    ...typography.styles.heading,
    color: colors.primary,
  },
  statLabel: {
    ...typography.styles.small,
    color: colors.textSecondary,
    marginTop: spacing.xs,
  },
  menuCard: {
    marginBottom: spacing.lg,
    padding: 0,
  },
  menuItem: {
    flexDirection: "row",
    alignItems: "center",
    padding: spacing.lg,
  },
  menuItemBorder: {
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  menuIcon: {
    fontSize: 20,
    marginRight: spacing.md,
  },
  menuLabel: {
    ...typography.styles.body,
    color: colors.textPrimary,
    flex: 1,
  },
  menuArrow: {
    fontSize: 20,
    color: colors.textTertiary,
  },
  infoCard: {
    marginBottom: spacing.lg,
  },
  infoTitle: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
    marginBottom: spacing.md,
  },
  infoRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingVertical: spacing.sm,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  infoLabel: {
    ...typography.styles.body,
    color: colors.textSecondary,
  },
  infoValue: {
    ...typography.styles.body,
    color: colors.textPrimary,
  },
  logoutButton: {
    borderColor: colors.error,
    marginTop: spacing.md,
  },
});

export default ProfileScreen;
