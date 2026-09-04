import { Navigate, Route, Routes } from "react-router-dom";
import Navbar from "./components/Navbar";
import RequireAuth from "./auth/RequireAuth";
import RequireAdmin from "./auth/RequireAdmin";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import DashboardPage from "./pages/DashboardPage";
import AccountsPage from "./pages/AccountsPage";
import CategoriesPage from "./pages/CategoriesPage";
import RecordIncomePage from "./pages/RecordIncomePage";
import RecordExpensePage from "./pages/RecordExpensePage";
import BudgetPage from "./pages/BudgetPage";
import YearlyReportPage from "./pages/YearlyReportPage";
import AdminUsersPage from "./pages/admin/AdminUsersPage";
import AdminSummaryPage from "./pages/admin/AdminSummaryPage";

export default function App() {
  return (
    <>
      <Navbar />
      <div className="container pb-5">
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          <Route element={<RequireAuth />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/accounts" element={<AccountsPage />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route path="/income/new" element={<RecordIncomePage />} />
            <Route path="/expenses/new" element={<RecordExpensePage />} />
            <Route path="/budget" element={<BudgetPage />} />
            <Route path="/reports/yearly" element={<YearlyReportPage />} />
          </Route>

          <Route element={<RequireAdmin />}>
            <Route path="/admin/users" element={<AdminUsersPage />} />
            <Route path="/admin/summary" element={<AdminSummaryPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </>
  );
}
