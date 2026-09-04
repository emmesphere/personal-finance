import { apiFetch } from "./client";
import type { AddExpenseRequest, AddExpenseResponse } from "../types/api";

export function addExpense(ledgerId: string, request: AddExpenseRequest) {
  return apiFetch<AddExpenseResponse>(`/api/ledgers/${ledgerId}/expenses`, {
    method: "POST",
    body: JSON.stringify(request),
  });
}
