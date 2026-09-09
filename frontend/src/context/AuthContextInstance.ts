import { createContext } from "react";
import type { AuthResponse } from "../types/auth";

export interface AuthContextValue {
    user: { userId: string; email: string; role: string | null } | null;
    token: string | null;
    isAuthenticated: boolean;
    login: (email: string, password: string) => Promise<void>;
    register: (
        email: string,
        password: string,
        confirmPassword: string,
    ) => Promise<void>;
    logout: () => Promise<void>;
    updateAuth: (result: AuthResponse) => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);