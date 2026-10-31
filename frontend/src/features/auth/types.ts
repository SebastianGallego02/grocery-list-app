/** Mirrors GroceryList.Application.Auth.LoginRequest. */
export interface LoginRequest {
  username: string
  password: string
}

/** Mirrors GroceryList.Application.Auth.RegisterRequest. */
export interface RegisterRequest {
  username: string
  fullName: string
  password: string
}

/** Mirrors GroceryList.Application.Auth.AuthResponse. */
export interface AuthResponse {
  token: string
  expiresAtUtc: string
  username: string
  fullName: string
}

/** Mirrors GroceryList.Application.Users.UserResponse. */
export interface UserResponse {
  id: string
  username: string
  fullName: string
}

export interface CurrentUser {
  username: string
  fullName: string
}
