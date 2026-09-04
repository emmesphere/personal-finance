import { apiFetch } from "./client";
import type { DashboardReport, YearlySummaryReport } from "../types/api";

export function getDashboard(ledgerId: string, year: number, month: number) {
  return apiFetch<DashboardReport>(
    `/api/ledgers/${ledgerId}/reports/dashboard?year=${year}&month=${month}`,
  );
}

export function getYearlySummary(ledgerId: string, year: number) {
  return apiFetch<YearlySummaryReport>(
    `/api/ledgers/${ledgerId}/reports/yearly-summary?year=${year}`,
  );
}
