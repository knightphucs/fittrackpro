// Main Tab Navigator
import React from "react";
import { Text } from "react-native";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { MainTabParamList } from "./types";
import { colors, spacing } from "../constants";

// Navigators
import FoodsNavigator from "./FoodsNavigator";

// Screens
import {
  HomeScreen,
  MealsScreen,
  ProgressScreen,
  ProfileScreen,
  WorkoutsScreen,
} from "../screens";

const Tab = createBottomTabNavigator<MainTabParamList>();

const MainTabNavigator: React.FC = () => {
  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textSecondary,
        tabBarStyle: {
          backgroundColor: colors.white,
          borderTopColor: colors.border,
          paddingTop: spacing.xs,
          paddingBottom: spacing.sm,
          height: 60,
        },
        tabBarLabelStyle: {
          fontSize: 12,
          fontWeight: "500",
        },
      }}
    >
      <Tab.Screen
        name="Home"
        component={HomeScreen}
        options={{
          tabBarLabel: "Trang chủ",
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 24 }}>🏠</Text>,
        }}
      />
      <Tab.Screen
        name="Foods"
        component={FoodsNavigator}
        options={{
          tabBarLabel: "Thực phẩm",
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 24 }}>🥗</Text>,
        }}
      />
      <Tab.Screen
        name="Meals"
        component={MealsScreen}
        options={{
          tabBarLabel: "Bữa ăn",
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 24 }}>🍽️</Text>,
        }}
      />
      <Tab.Screen
        name="Workouts"
        component={WorkoutsScreen}
        options={{
          tabBarLabel: "Bài tập",
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 24 }}>🏋️‍♂️</Text>,
        }}
      />
      <Tab.Screen
        name="Progress"
        component={ProgressScreen}
        options={{
          tabBarLabel: "Tiến trình",
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 24 }}>📈</Text>,
        }}
      />
      <Tab.Screen
        name="Profile"
        component={ProfileScreen}
        options={{
          tabBarLabel: "Hồ sơ",
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 24 }}>👤</Text>,
        }}
      />
    </Tab.Navigator>
  );
};

export default MainTabNavigator;
