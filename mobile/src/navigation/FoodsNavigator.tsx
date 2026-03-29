// Foods Stack Navigator
import React from "react";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { FoodsStackParamList } from "./types";
import { colors } from "../constants";

// Screens
import FoodListScreen from "../screens/foods/FoodListScreen";
// import FoodDetailScreen from '../screens/foods/FoodDetailScreen';

const Stack = createNativeStackNavigator<FoodsStackParamList>();

const FoodsNavigator: React.FC = () => {
  return (
    <Stack.Navigator
      screenOptions={{
        headerStyle: {
          backgroundColor: colors.white,
        },
        headerTintColor: colors.primary,
        headerTitleStyle: {
          fontWeight: "600",
        },
        animation: "slide_from_right",
      }}
    >
      <Stack.Screen
        name="FoodList"
        component={FoodListScreen}
        options={{
          headerShown: false,
        }}
      />
      {/*
      <Stack.Screen
        name="FoodDetail"
        component={FoodDetailScreen}
        options={{
          title: 'Chi tiết món ăn',
        }}
      />
      */}
    </Stack.Navigator>
  );
};

export default FoodsNavigator;
