import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./AuthContext";
import LoadingSpinner from "../components/LoadingSpinner";

export default function RequireAuth() {
  const { token, isLoading } = useAuth();

  if (isLoading) return <LoadingSpinner />;
  if (!token) return <Navigate to="/login" replace />;

  return <Outlet />;
}
