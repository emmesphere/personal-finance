import { apiFetch } from "./client";
import type { Account, CreateAccountRequest, CreateAccountResponse } from "../types/api";

export function listAccounts(ledgerId: string) {
  return apiFetch<Account[]>(`/api/ledgers/${ledgerId}/accounts`);
}

export function createAccount(ledgerId: string, request: CreateAccountRequest) {
  return apiFetch<CreateAccountResponse>(`/api/ledgers/${ledgerId}/accounts`, {
    method: "POST",
    body: JSON.stringify(request),
  });
}
