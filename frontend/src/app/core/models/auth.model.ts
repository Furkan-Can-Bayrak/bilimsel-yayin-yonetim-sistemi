export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
}

export interface AuthSession {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
}

export function displayName(firstName: string, lastName: string): string {
  return `${firstName} ${lastName}`.trim();
}
