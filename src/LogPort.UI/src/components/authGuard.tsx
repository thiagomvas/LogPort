import type { JSX } from "react";
import { Navigate } from "react-router-dom";

interface AuthGuardProps {
  children: JSX.Element;
}

// Checks localStorage for JWT token
export default function AuthGuard({ children }: AuthGuardProps) {
  const token = sessionStorage.getItem("jwtToken"); // or sessionStorage if you switched

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  return children;
}
