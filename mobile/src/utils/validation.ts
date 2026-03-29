// Validation utilities
import { config } from "../constants";

export interface ValidationResult {
  isValid: boolean;
  error?: string;
}

/**
 * Validate email format
 */
export const validateEmail = (email: string): ValidationResult => {
  if (!email) {
    return { isValid: false, error: "Email không được để trống" };
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) {
    return { isValid: false, error: "Email không hợp lệ" };
  }

  return { isValid: true };
};

/**
 * Validate password
 */
export const validatePassword = (password: string): ValidationResult => {
  if (!password) {
    return { isValid: false, error: "Mật khẩu không được để trống" };
  }

  if (password.length < config.validation.minPasswordLength) {
    return {
      isValid: false,
      error: `Mật khẩu phải có ít nhất ${config.validation.minPasswordLength} ký tự`,
    };
  }

  return { isValid: true };
};

/**
 * Validate name
 */
export const validateName = (
  name: string,
  fieldName: string = "Tên"
): ValidationResult => {
  if (!name || !name.trim()) {
    return { isValid: false, error: `${fieldName} không được để trống` };
  }

  if (name.length > config.validation.maxNameLength) {
    return {
      isValid: false,
      error: `${fieldName} không được vượt quá ${config.validation.maxNameLength} ký tự`,
    };
  }

  return { isValid: true };
};

/**
 * Validate required field
 */
export const validateRequired = (
  value: any,
  fieldName: string
): ValidationResult => {
  if (value === null || value === undefined || value === "") {
    return { isValid: false, error: `${fieldName} không được để trống` };
  }

  return { isValid: true };
};

/**
 * Validate positive number
 */
export const validatePositiveNumber = (
  value: number,
  fieldName: string
): ValidationResult => {
  if (isNaN(value) || value <= 0) {
    return { isValid: false, error: `${fieldName} phải là số dương` };
  }

  return { isValid: true };
};

/**
 * Validate number range
 */
export const validateNumberRange = (
  value: number,
  min: number,
  max: number,
  fieldName: string
): ValidationResult => {
  if (isNaN(value)) {
    return { isValid: false, error: `${fieldName} phải là số` };
  }

  if (value < min || value > max) {
    return { isValid: false, error: `${fieldName} phải từ ${min} đến ${max}` };
  }

  return { isValid: true };
};

/**
 * Validate login form
 */
export const validateLoginForm = (
  email: string,
  password: string
): ValidationResult => {
  const emailValidation = validateEmail(email);
  if (!emailValidation.isValid) return emailValidation;

  const passwordValidation = validatePassword(password);
  if (!passwordValidation.isValid) return passwordValidation;

  return { isValid: true };
};
