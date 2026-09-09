import { createContext } from "react";

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
}

export const AuthContext = createContext<AuthContextValue | null>(null);