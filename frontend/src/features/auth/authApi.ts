import { http } from '../../api/httpClient'
import type { AuthResponse, LoginRequest, RegisterRequest, UserResponse } from './types'

export function login(request: LoginRequest, signal?: AbortSignal): Promise<AuthResponse> {
  return http.post<AuthResponse>('/api/auth/login', { body: request, signal })
}

export function register(request: RegisterRequest, signal?: AbortSignal): Promise<AuthResponse> {
  return http.post<AuthResponse>('/api/auth/register', { body: request, signal })
}

export function getCurrentUser(signal?: AbortSignal): Promise<UserResponse> {
  return http.get<UserResponse>('/api/users/me', { signal })
}
