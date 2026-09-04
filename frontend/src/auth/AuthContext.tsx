import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { login as apiLogin, register as apiRegister } from "../api/auth";
import { setAuthToken, setUnauthorizedHandler } from "../api/client";
import { getMe } from "../api/me";
import type { LoginRequest, Me, RegisterRequest } from "../types/api";

const TOKEN_STORAGE_KEY = "personalfinance.token";

interface AuthState {
  token: string | null;
  me: Me | null;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  ledgerId: string | null;
  isAdmin: boolean;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    token: null,
    me: null,
    isLoading: true,
  });

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    setAuthToken(null);
    setState({ token: null, me: null, isLoading: false });
  }, []);

  useEffect(() => {
    setUnauthorizedHandler(logout);
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  useEffect(() => {
    const storedToken = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!storedToken) {
      setState((prev) => ({ ...prev, isLoading: false }));
      return;
    }

    setAuthToken(storedToken);
    getMe()
      .then((me) => setState({ token: storedToken, me, isLoading: false }))
      .catch(() => {
        localStorage.removeItem(TOKEN_STORAGE_KEY);
        setAuthToken(null);
        setState({ token: null, me: null, isLoading: false });
      });
  }, []);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await apiLogin(request);
    localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken);
    setAuthToken(response.accessToken);
    const me = await getMe();
    setState({ token: response.accessToken, me, isLoading: false });
  }, []);

  const register = useCallback(async (request: RegisterRequest) => {
    await apiRegister(request);
    await login({ username: request.username, password: request.password });
  }, [login]);

  const ledgerId = state.me?.ledgers[0]?.ledgerId ?? null;
  const isAdmin = state.me?.role === "Admin";

  return (
    <AuthContext.Provider value={{ ...state, ledgerId, isAdmin, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
