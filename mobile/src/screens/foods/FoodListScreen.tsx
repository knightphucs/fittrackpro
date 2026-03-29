// Food List Screen
import React, { useEffect, useState, useCallback } from "react";
import {
  View,
  FlatList,
  ActivityIndicator,
  StyleSheet,
  TextInput,
  Text,
  RefreshControl,
  Keyboard,
} from "react-native";
import { foodsApi } from "../../api";
import { Food } from "../../types";
import { colors, spacing, borderRadius, typography } from "../../constants";
import FoodCard from "../../components/food/FoodCard";

const FoodListScreen: React.FC = () => {
  const [foods, setFoods] = useState<Food[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [searchText, setSearchText] = useState("");

  const fetchFoods = useCallback(
    async (isRefresh = false) => {
      if (!isRefresh) setLoading(true);

      try {
        const data = await foodsApi.getFoods({
          searchTerm: searchText || undefined,
          pageNumber: 1,
        });

        console.log("Tổng số bản ghi:", data.totalCount);
        setFoods(data.items);
      } catch (error) {
        console.error("Lỗi tải món ăn:", error);
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [searchText]
  );

  useEffect(() => {
    fetchFoods();
  }, []);

  const handleSearch = () => {
    Keyboard.dismiss();
    fetchFoods();
  };

  const handleRefresh = () => {
    setRefreshing(true);
    fetchFoods(true);
  };

  const renderItem = useCallback(
    ({ item }: { item: Food }) => <FoodCard food={item} />,
    []
  );

  const keyExtractor = useCallback((item: Food) => item.id, []);

  const renderEmpty = () => (
    <View style={styles.emptyContainer}>
      <Text style={styles.emptyIcon}>🍽️</Text>
      <Text style={styles.emptyText}>
        {searchText
          ? `Không tìm thấy món "${searchText}"`
          : "Chưa có dữ liệu món ăn nào."}
      </Text>
    </View>
  );

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Thực phẩm</Text>
      </View>

      {/* Search Bar */}
      <View style={styles.searchContainer}>
        <TextInput
          style={styles.searchInput}
          placeholder="Tìm món ăn (Phở, Cơm...)"
          placeholderTextColor={colors.textTertiary}
          value={searchText}
          onChangeText={setSearchText}
          onSubmitEditing={handleSearch}
          returnKeyType="search"
          clearButtonMode="while-editing"
        />
      </View>

      {/* Content */}
      {loading ? (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Đang tải dữ liệu...</Text>
        </View>
      ) : (
        <FlatList
          data={foods}
          keyExtractor={keyExtractor}
          renderItem={renderItem}
          contentContainerStyle={styles.list}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={handleRefresh}
              colors={[colors.primary]}
              tintColor={colors.primary}
            />
          }
          ListEmptyComponent={renderEmpty}
        />
      )}
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
  },
  headerTitle: {
    ...typography.styles.title,
    color: colors.textPrimary,
  },
  searchContainer: {
    padding: spacing.lg,
    paddingTop: spacing.md,
    backgroundColor: colors.white,
  },
  searchInput: {
    backgroundColor: colors.background,
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.md,
    borderRadius: borderRadius.md,
    fontSize: typography.fontSize.lg,
    color: colors.textPrimary,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  loadingText: {
    marginTop: spacing.md,
    color: colors.textSecondary,
    fontSize: typography.fontSize.md,
  },
  list: {
    padding: spacing.lg,
    paddingBottom: spacing.xxl,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    paddingTop: 100,
  },
  emptyIcon: {
    fontSize: 48,
    marginBottom: spacing.md,
  },
  emptyText: {
    textAlign: "center",
    color: colors.textSecondary,
    fontSize: typography.fontSize.lg,
  },
});

export default FoodListScreen;
