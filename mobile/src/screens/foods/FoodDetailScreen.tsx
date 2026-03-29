// Food Detail Screen
import React, { useEffect, useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  Image,
  TouchableOpacity,
  Alert,
} from "react-native";
import { RouteProp, useRoute, useNavigation } from "@react-navigation/native";
import { foodsApi, mealsApi } from "../../api";
import { FoodDetail, MealType } from "../../types";
import {
  colors,
  spacing,
  borderRadius,
  typography,
  config,
} from "../../constants";
import { Card, Loading, Button } from "../../components";

type RouteParams = {
  FoodDetail: { foodId: string };
};

const FoodDetailScreen: React.FC = () => {
  const route = useRoute<RouteProp<RouteParams, "FoodDetail">>();
  const navigation = useNavigation();
  const { foodId } = route.params;

  const [food, setFood] = useState<FoodDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [servings, setServings] = useState(1);

  useEffect(() => {
    const fetchFood = async () => {
      try {
        const data = await foodsApi.getFoodById(foodId);
        setFood(data);
      } catch (error) {
        console.error("Error fetching food:", error);
        Alert.alert("Lỗi", "Không thể tải thông tin thực phẩm");
        navigation.goBack();
      } finally {
        setLoading(false);
      }
    };
    fetchFood();
  }, [foodId, navigation]);

  const handleAddToMeal = async (mealType: MealType) => {
    if (!food) return;
    try {
      await mealsApi.createMealLog({
        foodId: food.id,
        mealType,
        servings: servings,
        loggedAt: new Date().toISOString(),
      });
      Alert.alert("Thành công", "Đã thêm vào bữa ăn!", [
        { text: "OK", onPress: () => navigation.goBack() },
      ]);
    } catch (error) {
      Alert.alert("Lỗi", "Không thể thêm vào bữa ăn");
    }
  };

  const showMealPicker = () => {
    Alert.alert("Thêm vào bữa ăn", "Chọn bữa ăn:", [
      { text: "Bữa sáng", onPress: () => handleAddToMeal("Breakfast") },
      { text: "Bữa trưa", onPress: () => handleAddToMeal("Lunch") },
      { text: "Bữa tối", onPress: () => handleAddToMeal("Dinner") },
      { text: "Bữa phụ", onPress: () => handleAddToMeal("Snack") },
      { text: "Hủy", style: "cancel" },
    ]);
  };

  if (loading) {
    return <Loading fullScreen text="Đang tải..." />;
  }

  if (!food) {
    return null;
  }

  const calculated = {
    calories: Math.round(food.calories * servings),
    protein: Math.round(food.protein * servings * 10) / 10,
    carbs: Math.round(food.carbs * servings * 10) / 10,
    fat: Math.round(food.fat * servings * 10) / 10,
    fiber: Math.round((food.fiber || 0) * servings * 10) / 10,
    sugar: Math.round((food.sugar || 0) * servings * 10) / 10,
  };

  return (
    <ScrollView style={styles.container}>
      {/* Image */}
      <View style={styles.imageContainer}>
        <Image
          source={{
            uri: food.imageUrl || config.images.placeholderUrl,
          }}
          style={styles.image}
        />
        <TouchableOpacity
          style={styles.backButton}
          onPress={() => navigation.goBack()}
        >
          <Text style={styles.backIcon}>‹</Text>
        </TouchableOpacity>
      </View>

      {/* Info */}
      <View style={styles.content}>
        <Text style={styles.name}>{food.name}</Text>
        <Text style={styles.brand}>{food.brand || "Không rõ thương hiệu"}</Text>

        {/* Serving Selector */}
        <Card style={styles.servingCard}>
          <Text style={styles.servingLabel}>Khẩu phần</Text>
          <View style={styles.servingSelector}>
            <TouchableOpacity
              style={styles.servingButton}
              onPress={() => setServings(Math.max(0.5, servings - 0.5))}
            >
              <Text style={styles.servingButtonText}>−</Text>
            </TouchableOpacity>
            <Text style={styles.servingValue}>
              {servings} ({food.servingSize}
              {food.servingUnit})
            </Text>
            <TouchableOpacity
              style={styles.servingButton}
              onPress={() => setServings(servings + 0.5)}
            >
              <Text style={styles.servingButtonText}>+</Text>
            </TouchableOpacity>
          </View>
        </Card>

        {/* Calories Highlight */}
        <Card style={styles.caloriesCard}>
          <Text style={styles.caloriesValue}>{calculated.calories}</Text>
          <Text style={styles.caloriesLabel}>Calories</Text>
        </Card>

        {/* Macros */}
        <View style={styles.macrosRow}>
          <MacroItem
            label="Protein"
            value={calculated.protein}
            unit="g"
            color={colors.protein}
          />
          <MacroItem
            label="Carbs"
            value={calculated.carbs}
            unit="g"
            color={colors.carbs}
          />
          <MacroItem
            label="Fat"
            value={calculated.fat}
            unit="g"
            color={colors.fat}
          />
        </View>

        {/* Additional Nutrients */}
        <Card style={styles.nutrientsCard}>
          <Text style={styles.nutrientsTitle}>Dinh dưỡng chi tiết</Text>
          <NutrientRow label="Chất xơ" value={`${calculated.fiber}g`} />
          <NutrientRow label="Đường" value={`${calculated.sugar}g`} />
        </Card>

        {/* Add Button */}
        <Button
          title="🍽️ Thêm vào bữa ăn"
          onPress={showMealPicker}
          style={styles.addButton}
          size="large"
        />
      </View>
    </ScrollView>
  );
};

// Macro Item Component
const MacroItem: React.FC<{
  label: string;
  value: number;
  unit: string;
  color: string;
}> = ({ label, value, unit, color }) => (
  <Card style={styles.macroCard}>
    <View style={[styles.macroDot, { backgroundColor: color }]} />
    <Text style={styles.macroValue}>
      {value}
      <Text style={styles.macroUnit}>{unit}</Text>
    </Text>
    <Text style={styles.macroLabel}>{label}</Text>
  </Card>
);

// Nutrient Row Component
const NutrientRow: React.FC<{ label: string; value: string }> = ({
  label,
  value,
}) => (
  <View style={styles.nutrientRow}>
    <Text style={styles.nutrientLabel}>{label}</Text>
    <Text style={styles.nutrientValue}>{value}</Text>
  </View>
);

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  imageContainer: {
    position: "relative",
    height: 250,
  },
  image: {
    width: "100%",
    height: "100%",
    backgroundColor: colors.border,
  },
  backButton: {
    position: "absolute",
    top: 50,
    left: spacing.lg,
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: "rgba(0,0,0,0.5)",
    justifyContent: "center",
    alignItems: "center",
  },
  backIcon: {
    fontSize: 28,
    color: colors.white,
    marginTop: -2,
  },
  content: {
    padding: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  name: {
    ...typography.styles.title,
    color: colors.textPrimary,
  },
  brand: {
    ...typography.styles.body,
    color: colors.textSecondary,
    marginTop: spacing.xs,
    marginBottom: spacing.lg,
  },
  servingCard: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    marginBottom: spacing.lg,
  },
  servingLabel: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
  },
  servingSelector: {
    flexDirection: "row",
    alignItems: "center",
  },
  servingButton: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: colors.primary,
    justifyContent: "center",
    alignItems: "center",
  },
  servingButtonText: {
    fontSize: 20,
    color: colors.white,
    fontWeight: "bold",
  },
  servingValue: {
    ...typography.styles.body,
    color: colors.textPrimary,
    marginHorizontal: spacing.md,
  },
  caloriesCard: {
    alignItems: "center",
    backgroundColor: colors.primary,
    marginBottom: spacing.lg,
  },
  caloriesValue: {
    fontSize: 48,
    fontWeight: "bold",
    color: colors.white,
  },
  caloriesLabel: {
    ...typography.styles.body,
    color: "rgba(255,255,255,0.8)",
  },
  macrosRow: {
    flexDirection: "row",
    marginBottom: spacing.lg,
  },
  macroCard: {
    flex: 1,
    alignItems: "center",
    marginHorizontal: spacing.xs,
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
  macroUnit: {
    fontSize: 12,
    fontWeight: "normal",
  },
  macroLabel: {
    ...typography.styles.small,
    color: colors.textSecondary,
  },
  nutrientsCard: {
    marginBottom: spacing.lg,
  },
  nutrientsTitle: {
    ...typography.styles.bodyBold,
    color: colors.textPrimary,
    marginBottom: spacing.md,
  },
  nutrientRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingVertical: spacing.sm,
    borderBottomWidth: 1,
    borderBottomColor: colors.borderLight,
  },
  nutrientLabel: {
    ...typography.styles.body,
    color: colors.textSecondary,
  },
  nutrientValue: {
    ...typography.styles.body,
    color: colors.textPrimary,
  },
  addButton: {
    marginTop: spacing.md,
  },
});

export default FoodDetailScreen;
