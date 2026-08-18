export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
}

export interface AuthSession {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
}
