// src/app/(home)/rbac/RBACContext.tsx
"use client";

import React, {
  createContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
} from "react";
import { useSession } from "next-auth/react";
import { Session } from "next-auth";
import api from "@/app/lib/api"; // Your existing api instance with interceptor
import axios from "axios";

// Define the User type based on your API response
interface User {
  id: number;
  name: string;
  email: string;
  username: string | null;
  isSysAdmin: boolean;
  isArchived: boolean;
  isActive: boolean;
  role: string;
}

// Define the context type
interface RBACContextType {
  user: User | null;
  setUser: React.Dispatch<React.SetStateAction<User | null>>;
  refreshUser: () => void;
  session: Session | null;
}

// Create the context with proper typing
export const RBACContext = createContext<RBACContextType | undefined>(
  undefined
);

export function RBACProvider({ children }: { children: ReactNode }) {
  const { data: session, status } = useSession();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  const disableAuth =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  const fetchDevUser = useCallback(async () => {
    try {
      const response = await api.get("/users/GetLocalDevUser");
      const userData: User = {
        ...response.data,
        role: response.data.isSysAdmin ? "sysAdmin" : "viewer",
      };
      setUser(userData);
    } catch (error) {
      console.error("Failed to fetch dev user: ", error);
      if (axios.isAxiosError(error)) {
        console.error("Error details:", error.response?.data);
      }
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchUserData = useCallback(async () => {
    try {
      const response = await api.get("/users/GetCurrentUser");
      const userData: User = {
        ...response.data,
        role: response.data.isSysAdmin ? "sysAdmin" : "viewer",
      };
      setUser(userData);
    } catch (error) {
      console.error("Failed to fetch user data:", error);
      if (axios.isAxiosError(error)) {
        console.error("Error details:", error.response?.data);
      }
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch user data when session is available
  useEffect(() => {
    if (disableAuth) {
      fetchDevUser();
    } else if (status === "authenticated") {
      fetchUserData();
    } else if (status === "unauthenticated") {
      setUser(null);
      setLoading(false);
    }
  }, [status, fetchUserData, fetchDevUser, disableAuth]);

  // Refresh user data if needed
  const refreshUser = useCallback(() => {
    if (disableAuth) {
      setLoading(true);
      fetchDevUser();
    } else if (status === "authenticated") {
      setLoading(true);
      fetchUserData();
    }
  }, [status, fetchUserData, fetchDevUser, disableAuth]);

  // Show loading state while checking auth
  if (status === "loading" || loading) {
    return (
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "100vh",
        }}
      >
        Loading RBAC...
      </div>
    );
  }

  // Not authenticated - let NextAuth handle redirect
  if (status === "unauthenticated" && !disableAuth) {
    return <>{children}</>;
  }

  // Failed to load user
  if (!user && (status === "authenticated" || disableAuth)) {
    return (
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "100vh",
          flexDirection: "column",
          gap: "16px",
        }}
      >
        <div>Failed to load user data</div>
        <button onClick={refreshUser}>Retry</button>
      </div>
    );
  }

  return (
    <RBACContext.Provider value={{ user, setUser, refreshUser, session }}>
      {children}
    </RBACContext.Provider>
  );
}
