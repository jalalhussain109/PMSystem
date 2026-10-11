export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

export interface AuthUser {
  id: string;
  userName: string;
  roles: string[];
  permissions: string[];
}

export interface AuthProfile {
  id: number;
  userName: string | null;
  displayName: string | null;
  email: string | null;
  phoneNumber: string | null;
  isActive: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
