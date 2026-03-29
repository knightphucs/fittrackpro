// Login Screen
import React, { useState } from "react";
import {
  StyleSheet,
  Text,
  View,
  TextInput,
  TouchableOpacity,
  Alert,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { useAuth } from "../../contexts";
import { colors, spacing, borderRadius, typography } from "../../constants";
import { validateLoginForm } from "../../utils";

const LoginScreen: React.FC = () => {
  const { login, isLoading } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [localLoading, setLocalLoading] = useState(false);

  const loading = isLoading || localLoading;

  const handleLogin = async () => {
    // Validate form
    const validation = validateLoginForm(email, password);
    if (!validation.isValid) {
      Alert.alert("Lỗi", validation.error);
      return;
    }

    setLocalLoading(true);
    try {
      await login({ email, password });
      Alert.alert("Thành công", "Đăng nhập thành công!");
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.error || "Đăng nhập thất bại. Vui lòng thử lại.";
      Alert.alert("Đăng nhập thất bại", errorMessage);
    } finally {
      setLocalLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === "ios" ? "padding" : "height"}
      style={styles.container}
    >
      <View style={styles.formContainer}>
        <Text style={styles.title}>FitTrack Pro</Text>
        <Text style={styles.subtitle}>Welcome back!</Text>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Email</Text>
          <TextInput
            style={styles.input}
            placeholder="example@email.com"
            placeholderTextColor={colors.textTertiary}
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            autoComplete="email"
            editable={!loading}
          />
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Password</Text>
          <TextInput
            style={styles.input}
            placeholder="Enter your password"
            placeholderTextColor={colors.textTertiary}
            value={password}
            onChangeText={setPassword}
            secureTextEntry
            autoComplete="password"
            editable={!loading}
          />
        </View>

        <TouchableOpacity
          style={[styles.button, loading && styles.buttonDisabled]}
          onPress={handleLogin}
          disabled={loading}
          activeOpacity={0.8}
        >
          {loading ? (
            <ActivityIndicator color={colors.white} />
          ) : (
            <Text style={styles.buttonText}>Đăng nhập</Text>
          )}
        </TouchableOpacity>

        {/* Future: Add register and forgot password links */}
        {/*
        <View style={styles.linksContainer}>
          <TouchableOpacity>
            <Text style={styles.link}>Quên mật khẩu?</Text>
          </TouchableOpacity>
          <TouchableOpacity>
            <Text style={styles.link}>Đăng ký tài khoản</Text>
          </TouchableOpacity>
        </View>
        */}
      </View>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.white,
    justifyContent: "center",
    padding: spacing.xl,
  },
  formContainer: {
    width: "100%",
  },
  title: {
    ...typography.styles.hero,
    color: colors.primary,
    marginBottom: spacing.sm,
    textAlign: "center",
  },
  subtitle: {
    ...typography.styles.subheading,
    color: colors.textSecondary,
    marginBottom: spacing.xxxl,
    textAlign: "center",
    fontWeight: "400",
  },
  inputContainer: {
    marginBottom: spacing.lg,
  },
  label: {
    ...typography.styles.caption,
    fontWeight: "600",
    color: colors.textPrimary,
    marginBottom: spacing.sm,
  },
  input: {
    height: 50,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: borderRadius.sm,
    paddingHorizontal: spacing.lg,
    fontSize: typography.fontSize.lg,
    backgroundColor: colors.background,
    color: colors.textPrimary,
  },
  button: {
    height: 50,
    backgroundColor: colors.primary,
    borderRadius: borderRadius.sm,
    justifyContent: "center",
    alignItems: "center",
    marginTop: spacing.lg,
  },
  buttonDisabled: {
    backgroundColor: colors.primaryLight,
  },
  buttonText: {
    color: colors.white,
    fontSize: typography.fontSize.lg,
    fontWeight: "bold",
  },
  linksContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginTop: spacing.xl,
  },
  link: {
    color: colors.primary,
    fontSize: typography.fontSize.md,
  },
});

export default LoginScreen;
