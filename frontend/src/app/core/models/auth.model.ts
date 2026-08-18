export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  username: string;
  role: string;
  expiresAtUtc: string;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
  postCount: number;
}
