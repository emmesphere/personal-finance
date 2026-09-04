import { apiFetch } from "./client";
import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from "../types/api";

export function register(request: RegisterRequest) {
  return apiFetch<RegisterResponse>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify(request),
  });
}

export function login(request: LoginRequest) {
  return apiFetch<LoginResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify(request),
  });
}
