import { useState, type ReactNode } from "react";
import * as authApi from "../api/authApi";
import type { AuthResponse } from "../types/auth";
import { AuthContext, type AuthContextValue } from "./AuthContextInstance";

const STORAGE_KEY = "auth";

function readStoredAuth(): AuthResponse | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  const parsed: AuthResponse = JSON.parse(raw);
  if (new Date(parsed.expiration) <= new Date()) {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
  return parsed;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthResponse | null>(() => readStoredAuth());

  function storeAuth(result: AuthResponse) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(result));
    setAuth(result);
  }

  function updateAuth(result: AuthResponse) {
    storeAuth(result);
  }

  async function login(email: string, password: string) {
    const result = await authApi.login(email, password);
    storeAuth(result);
  }

  async function register(
    email: string,
    password: string,
    confirmPassword: string,
  ) {
    const result = await authApi.register(email, password, confirmPassword);
    storeAuth(result);
  }

  async function logout() {
    if (auth) {
      await authApi.logout(auth.token).catch(() => {});
    }
    localStorage.removeItem(STORAGE_KEY);
    setAuth(null);
  }

  const value: AuthContextValue = {
    user: auth
      ? { userId: auth.userId, email: auth.email, role: auth.role }
      : null,
    token: auth?.token ?? null,
    isAuthenticated: !!auth,
    login,
    register,
    logout,
    updateAuth,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
