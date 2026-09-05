import { apiFetch } from "./client";
import type { Me } from "../types/api";

export function getMe() {
  return apiFetch<Me>("/api/me");
}
