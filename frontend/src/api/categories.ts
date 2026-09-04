import { apiFetch } from "./client";
import type {
  Category,
  CategoryKind,
  CreateCategoryRequest,
  CreateCategoryResponse,
} from "../types/api";

export function listCategories(kind?: CategoryKind) {
  const query = kind ? `?kind=${kind}` : "";
  return apiFetch<Category[]>(`/api/categories${query}`);
}

export function createCategory(request: CreateCategoryRequest) {
  return apiFetch<CreateCategoryResponse>("/api/categories", {
    method: "POST",
    body: JSON.stringify(request),
  });
}

export function deactivateCategory(categoryId: string) {
  return apiFetch<void>(`/api/categories/${categoryId}/deactivate`, {
    method: "PATCH",
  });
}
