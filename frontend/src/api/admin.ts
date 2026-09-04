import { apiFetch } from "./client";
import type { AdminSummary, AdminUser } from "../types/api";

export function listUsers() {
  return apiFetch<AdminUser[]>("/api/admin/users");
}

export function deactivateUser(userId: string) {
  return apiFetch<void>(`/api/admin/users/${userId}/deactivate`, { method: "PATCH" });
}

export function promoteUser(userId: string) {
  return apiFetch<void>(`/api/admin/users/${userId}/promote`, { method: "PATCH" });
}

export function demoteUser(userId: string) {
  return apiFetch<void>(`/api/admin/users/${userId}/demote`, { method: "PATCH" });
}

export function getAdminSummary() {
  return apiFetch<AdminSummary>("/api/admin/summary");
}
