// Navigation type definitions
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { BottomTabScreenProps } from "@react-navigation/bottom-tabs";
import {
  CompositeScreenProps,
  NavigatorScreenParams,
} from "@react-navigation/native";

// Root Stack (Auth + Main)
export type RootStackParamList = {
  Auth: NavigatorScreenParams<AuthStackParamList>;
  Main: NavigatorScreenParams<MainTabParamList>;
};

// Auth Stack
export type AuthStackParamList = {
  Login: undefined;
  Register: undefined;
  ForgotPassword: undefined;
};

// Main Tab Navigator
export type MainTabParamList = {
  Home: undefined;
  Foods: NavigatorScreenParams<FoodsStackParamList>;
  Meals: NavigatorScreenParams<MealsStackParamList>;
  Workouts: NavigatorScreenParams<WorkoutStackParamList>;
  Progress: NavigatorScreenParams<ProgressStackParamList>;
  Profile: NavigatorScreenParams<ProfileStackParamList>;
};

// Foods Stack
export type FoodsStackParamList = {
  FoodList: undefined;
  FoodDetail: { foodId: string };
  FoodSearch: undefined;
};

// Meals Stack
export type MealsStackParamList = {
  MealLog: undefined;
  AddMeal: { mealType?: string; date?: string };
  MealDetail: { mealId: string };
};

// Progress Stack
export type ProgressStackParamList = {
  ProgressHistory: undefined;
  ProgressDetail: { entryId: string };
  ProgressPhotos: undefined;
  AddProgress: undefined;
};

export type WorkoutStackParamList = {
  WorkoutList: undefined;
  WorkoutDetail: { workoutId: string };
};

// Profile Stack
export type ProfileStackParamList = {
  ProfileMain: undefined;
  EditProfile: undefined;
  Goals: undefined;
  EditGoal: { goalId?: string };
  Settings: undefined;
};

// Screen Props Types
export type RootStackScreenProps<T extends keyof RootStackParamList> =
  NativeStackScreenProps<RootStackParamList, T>;

export type AuthStackScreenProps<T extends keyof AuthStackParamList> =
  CompositeScreenProps<
    NativeStackScreenProps<AuthStackParamList, T>,
    RootStackScreenProps<keyof RootStackParamList>
  >;

export type MainTabScreenProps<T extends keyof MainTabParamList> =
  CompositeScreenProps<
    BottomTabScreenProps<MainTabParamList, T>,
    RootStackScreenProps<keyof RootStackParamList>
  >;

export type FoodsStackScreenProps<T extends keyof FoodsStackParamList> =
  CompositeScreenProps<
    NativeStackScreenProps<FoodsStackParamList, T>,
    MainTabScreenProps<"Foods">
  >;

export type MealsStackScreenProps<T extends keyof MealsStackParamList> =
  CompositeScreenProps<
    NativeStackScreenProps<MealsStackParamList, T>,
    MainTabScreenProps<"Meals">
  >;

export type WorkoutStackScreenProps<T extends keyof WorkoutStackParamList> =
  CompositeScreenProps<
    NativeStackScreenProps<WorkoutStackParamList, T>,
    MainTabScreenProps<"Workouts">
  >;

export type ProgressStackScreenProps<T extends keyof ProgressStackParamList> =
  CompositeScreenProps<
    NativeStackScreenProps<ProgressStackParamList, T>,
    MainTabScreenProps<"Progress">
  >;

export type ProfileStackScreenProps<T extends keyof ProfileStackParamList> =
  CompositeScreenProps<
    NativeStackScreenProps<ProfileStackParamList, T>,
    MainTabScreenProps<"Profile">
  >;

// Declaration for useNavigation hook
declare global {
  namespace ReactNavigation {
    interface RootParamList extends RootStackParamList {}
  }
}
