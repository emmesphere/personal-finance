import { apiFetch } from "./client";
import type { SetMonthlyBudgetRequest, SetMonthlyBudgetResponse } from "../types/api";

export function setMonthlyBudget(
  ledgerId: string,
  year: number,
  month: number,
  request: SetMonthlyBudgetRequest,
) {
  return apiFetch<SetMonthlyBudgetResponse>(
    `/api/ledgers/${ledgerId}/budgets/${year}/${month}`,
    {
      method: "PUT",
      body: JSON.stringify(request),
    },
  );
}
