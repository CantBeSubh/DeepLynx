// src/app/(home)/rbac/RBACContext.tsx
"use client";

import React, { createContext, useState, useEffect, ReactNode } from "react";
import { useSession } from "next-auth/react";
import { Session } from "next-auth";
import api from "@/app/lib/api"; // Your existing api instance with interceptor

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

  // Fetch user data when session is available
  useEffect(() => {
    if (status === "authenticated") {
      fetchUserData();
    } else if (status === "unauthenticated") {
      setUser(null);
      setLoading(false);
    }
  }, [status]);

  const fetchUserData = async () => {
    try {
      // Call the new /users/me endpoint which gets user from JWT token
      const response = await api.get("/users/GetCurrentUser");

      // Add role field based on isSysAdmin
      // Later you can get this from the API response if backend provides it
      const userData: User = {
        ...response.data,
        role: response.data.isSysAdmin ? "sysAdmin" : "viewer",
      };

      setUser(userData);
    } catch (error) {
      console.error("Failed to fetch user data:", error);
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  // Refresh user data if needed
  const refreshUser = () => {
    if (status === "authenticated") {
      setLoading(true);
      fetchUserData();
    }
  };

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
        Loading...
      </div>
    );
  }

  // Not authenticated - let NextAuth handle redirect
  if (status === "unauthenticated") {
    return <>{children}</>; // Let your app's auth guards handle this
  }

  // Failed to load user
  if (!user && status === "authenticated") {
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
