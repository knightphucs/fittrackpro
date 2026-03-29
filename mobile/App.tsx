// App.tsx - Main entry point
import React from "react";
import { StatusBar } from "react-native";
import { AuthProvider, ThemeProvider } from "./src/contexts";
import { AppNavigator } from "./src/navigation";
import { colors } from "./src/constants";

export default function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <StatusBar barStyle="dark-content" backgroundColor={colors.white} />
        <AppNavigator />
      </AuthProvider>
    </ThemeProvider>
  );
}
