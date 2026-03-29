// Types for Authentication
export interface AuthResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}
