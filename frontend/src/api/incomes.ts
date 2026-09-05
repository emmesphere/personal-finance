import { apiFetch } from "./client";
import type { AddIncomeRequest, AddIncomeResponse } from "../types/api";

export function addIncome(ledgerId: string, request: AddIncomeRequest) {
  return apiFetch<AddIncomeResponse>(`/api/ledgers/${ledgerId}/incomes`, {
    method: "POST",
    body: JSON.stringify(request),
  });
}
