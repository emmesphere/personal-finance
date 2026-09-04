export type AccountType =
  | "BankAccount"
  | "Wallet"
  | "Benefit"
  | "CreditCard"
  | "Debit"
  | "Loan";

export type CategoryKind = "Income" | "Expense";

export type UserRole = "User" | "Admin";

export interface LedgerMembership {
  ledgerId: string;
  ledgerName: string;
  isOwner: boolean;
}

export interface Me {
  id: string;
  fullName: string;
  username: string;
  email: string | null;
  phoneNumber: string | null;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
  ledgers: LedgerMembership[];
}

export interface RegisterRequest {
  fullName: string;
  username: string;
  email?: string;
  phoneNumber?: string;
  password: string;
}

export interface RegisterResponse {
  userId: string;
  ledgerId: string;
  username: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  userId: string;
  username: string;
  role: UserRole;
}

export interface Account {
  id: string;
  name: string;
  type: AccountType;
  dueDateDay: number | null;
  isActive: boolean;
  balance: number;
}

export interface CreateAccountRequest {
  name: string;
  type: AccountType;
  dueDateDay?: number;
  openingBalance?: number;
}

export interface CreateAccountResponse {
  accountId: string;
}

export interface Category {
  id: string;
  name: string;
  kind: CategoryKind;
  isSystemDefined: boolean;
}

export interface CreateCategoryRequest {
  name: string;
  kind: CategoryKind;
}

export interface CreateCategoryResponse {
  categoryId: string;
}

export interface AddIncomeRequest {
  categoryId: string;
  receivingAccountId: string;
  amount: number;
  date: string;
  description?: string;
}

export interface AddIncomeResponse {
  journalEntryId: string;
}

export interface AddExpenseRequest {
  categoryId: string;
  paymentAccountId: string;
  amount: number;
  date: string;
  description?: string;
  installmentCount?: number;
}

export interface AddExpenseResponse {
  journalEntryId: string;
  installmentPlanId: string | null;
  installmentCount: number;
}

export interface SetMonthlyBudgetRequest {
  amount: number;
}

export interface SetMonthlyBudgetResponse {
  monthlyBudgetId: string;
}

export interface CategoryExpense {
  categoryId: string;
  categoryName: string;
  amount: number;
}

export interface DashboardReport {
  totalBalance: number;
  totalExpenses: number;
  budgetAmount: number | null;
  expensesByCategory: CategoryExpense[];
}

export interface MonthlyExpense {
  month: number;
  amount: number;
}

export interface YearlySummaryReport {
  year: number;
  months: MonthlyExpense[];
}

export interface AdminUser {
  id: string;
  fullName: string;
  username: string;
  email: string | null;
  phoneNumber: string | null;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
}

export interface AdminSummary {
  totalUsers: number;
  totalLedgers: number;
  postedJournalEntriesThisMonth: number;
}
