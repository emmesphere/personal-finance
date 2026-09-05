import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./AuthContext";
import LoadingSpinner from "../components/LoadingSpinner";

export default function RequireAdmin() {
  const { token, isAdmin, isLoading } = useAuth();

  if (isLoading) return <LoadingSpinner />;
  if (!token) return <Navigate to="/login" replace />;
  if (!isAdmin) return <Navigate to="/" replace />;

  return <Outlet />;
}
