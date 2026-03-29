// Register Screen
import React, { useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  Alert,
} from "react-native";
import { useNavigation } from "@react-navigation/native";
import { useAuth } from "../../contexts";
import { colors, spacing, borderRadius, typography } from "../../constants";
import { Button, Input, Card } from "../../components";

const RegisterScreen: React.FC = () => {
  const navigation = useNavigation();
  const { register, isLoading } = useAuth();
  const [form, setForm] = useState({
    email: "",
    password: "",
    confirmPassword: "",
    firstName: "",
    lastName: "",
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const newErrors: Record<string, string> = {};

    if (!form.firstName.trim()) {
      newErrors.firstName = "Vui lòng nhập họ";
    }
    if (!form.lastName.trim()) {
      newErrors.lastName = "Vui lòng nhập tên";
    }
    if (!form.email.trim()) {
      newErrors.email = "Vui lòng nhập email";
    } else if (!/\S+@\S+\.\S+/.test(form.email)) {
      newErrors.email = "Email không hợp lệ";
    }
    if (!form.password) {
      newErrors.password = "Vui lòng nhập mật khẩu";
    } else if (form.password.length < 6) {
      newErrors.password = "Mật khẩu phải có ít nhất 6 ký tự";
    }
    if (form.password !== form.confirmPassword) {
      newErrors.confirmPassword = "Mật khẩu không khớp";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleRegister = async () => {
    if (!validate()) return;

    try {
      await register({
        email: form.email,
        password: form.password,
        firstName: form.firstName,
        lastName: form.lastName,
      });
    } catch (error: any) {
      Alert.alert(
        "Đăng ký thất bại",
        error?.message || "Có lỗi xảy ra, vui lòng thử lại"
      );
    }
  };

  const updateForm = (field: string, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: "" }));
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === "ios" ? "padding" : "height"}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        {/* Header */}
        <View style={styles.header}>
          <TouchableOpacity
            onPress={() => navigation.goBack()}
            style={styles.backButton}
          >
            <Text style={styles.backIcon}>‹</Text>
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Đăng ký</Text>
        </View>

        {/* Form */}
        <Card style={styles.formCard}>
          <View style={styles.nameRow}>
            <View style={styles.nameField}>
              <Input
                label="Họ"
                placeholder="Nguyễn"
                value={form.firstName}
                onChangeText={(value) => updateForm("firstName", value)}
                error={errors.firstName}
                autoCapitalize="words"
              />
            </View>
            <View style={styles.nameField}>
              <Input
                label="Tên"
                placeholder="Văn A"
                value={form.lastName}
                onChangeText={(value) => updateForm("lastName", value)}
                error={errors.lastName}
                autoCapitalize="words"
              />
            </View>
          </View>

          <Input
            label="Email"
            placeholder="email@example.com"
            value={form.email}
            onChangeText={(value) => updateForm("email", value)}
            keyboardType="email-address"
            autoCapitalize="none"
            error={errors.email}
          />

          <Input
            label="Mật khẩu"
            placeholder="••••••••"
            value={form.password}
            onChangeText={(value) => updateForm("password", value)}
            secureTextEntry
            error={errors.password}
          />

          <Input
            label="Xác nhận mật khẩu"
            placeholder="••••••••"
            value={form.confirmPassword}
            onChangeText={(value) => updateForm("confirmPassword", value)}
            secureTextEntry
            error={errors.confirmPassword}
          />

          <Button
            title={isLoading ? "Đang xử lý..." : "Đăng ký"}
            onPress={handleRegister}
            disabled={isLoading}
            style={styles.registerButton}
            size="large"
          />
        </Card>

        {/* Footer */}
        <View style={styles.footer}>
          <Text style={styles.footerText}>Đã có tài khoản? </Text>
          <TouchableOpacity onPress={() => navigation.goBack()}>
            <Text style={styles.footerLink}>Đăng nhập</Text>
          </TouchableOpacity>
        </View>

        {/* Terms */}
        <Text style={styles.termsText}>
          Bằng việc đăng ký, bạn đồng ý với{" "}
          <Text style={styles.termsLink}>Điều khoản dịch vụ</Text> và{" "}
          <Text style={styles.termsLink}>Chính sách bảo mật</Text>
        </Text>
      </ScrollView>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scrollContent: {
    padding: spacing.lg,
    paddingTop: 50,
  },
  header: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: spacing.xl,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: colors.white,
    justifyContent: "center",
    alignItems: "center",
    marginRight: spacing.md,
  },
  backIcon: {
    fontSize: 28,
    color: colors.textPrimary,
    marginTop: -2,
  },
  headerTitle: {
    ...typography.styles.title,
    color: colors.textPrimary,
  },
  formCard: {
    marginBottom: spacing.xl,
  },
  nameRow: {
    flexDirection: "row",
    marginHorizontal: -spacing.xs,
  },
  nameField: {
    flex: 1,
    marginHorizontal: spacing.xs,
  },
  registerButton: {
    marginTop: spacing.md,
  },
  footer: {
    flexDirection: "row",
    justifyContent: "center",
    marginBottom: spacing.lg,
  },
  footerText: {
    ...typography.styles.body,
    color: colors.textSecondary,
  },
  footerLink: {
    ...typography.styles.body,
    color: colors.primary,
    fontWeight: "600",
  },
  termsText: {
    ...typography.styles.small,
    color: colors.textTertiary,
    textAlign: "center",
    lineHeight: 20,
  },
  termsLink: {
    color: colors.primary,
  },
});

export default RegisterScreen;
