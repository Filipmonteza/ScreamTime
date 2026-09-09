import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import type { Role } from "./Navbar";

type ProtectedRouteProps = {
  roles?: Role[];
  children: ReactNode;
};

function ProtectedRoute({ roles, children }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && !roles.includes(user?.role as Role)) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}

export default ProtectedRoute;
