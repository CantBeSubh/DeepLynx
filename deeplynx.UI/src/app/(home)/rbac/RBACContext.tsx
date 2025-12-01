// src/app/(home)/rbac/RBACContext.tsx
"use client";

import React, {
  createContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
} from "react";
import { Session } from "next-auth";
import Image from "next/image";
import {
  getCurrentUser,
  getLocalDevUser,
} from "@/app/lib/client_service/user_services.client";
import { UserResponseDto } from "../types/responseDTOs";
import { useSafeSession } from "@/app/hooks/useSafeSession";

interface RBACContextType {
  user: UserResponseDto | null;
  setUser: React.Dispatch<React.SetStateAction<UserResponseDto | null>>;
  refreshUser: () => void;
  session: Session | null;
}

export const RBACContext = createContext<RBACContextType | undefined>(
  undefined
);

export function RBACProvider({ children }: { children: ReactNode }) {
  const disableAuth =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  const { data: session, status } = useSafeSession();
  const [user, setUser] = useState<UserResponseDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDevUser = async () => {
      try {
        const response = await getLocalDevUser();
        const userData: UserResponseDto = {
          ...response,
          role: response.isSysAdmin ? "sysAdmin" : "viewer",
        };
        setUser(userData);
      } catch (error) {
        console.error("Failed to fetch dev user:", error);
        if (error instanceof Error) {
          console.error("Error details:", error.message);
        }
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    const fetchUserData = async () => {
      try {
        const response = await getCurrentUser();
        const userData: UserResponseDto = {
          ...response,
          role: response.isSysAdmin ? "sysAdmin" : "viewer",
        };
        setUser(userData);
      } catch (error) {
        console.error("Failed to fetch user data:", error);
        if (error instanceof Error) {
          console.error("Error details:", error.message);
        }
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    if (disableAuth) {
      fetchDevUser();
    } else if (status === "authenticated") {
      fetchUserData();
    } else if (status === "unauthenticated") {
      setUser(null);
      setLoading(false);
    }
  }, [status, disableAuth]);

  const refreshUser = useCallback(() => {
    const fetchDevUser = async () => {
      setLoading(true);
      try {
        const response = await getLocalDevUser();
        const userData: UserResponseDto = {
          ...response,
          role: response.isSysAdmin ? "sysAdmin" : "viewer",
        };
        setUser(userData);
      } catch (error) {
        console.error("Failed to fetch dev user:", error);
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    const fetchUserData = async () => {
      setLoading(true);
      try {
        const response = await getCurrentUser();
        const userData: UserResponseDto = {
          ...response,
          role: response.isSysAdmin ? "sysAdmin" : "viewer",
        };
        setUser(userData);
      } catch (error) {
        console.error("Failed to fetch user data:", error);
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    if (disableAuth) {
      fetchDevUser();
    } else if (status === "authenticated") {
      fetchUserData();
    }
  }, [status, disableAuth]);

  if (loading) {
    return (
      <div
        className="login"
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "100vh",
        }}
      >
        <Image
          src="/assets/nexusWhite.png"
          alt="DeepLynx logo"
          width={265.8}
          height={113.9}
          priority
        />
      </div>
    );
  }

  if (status === "unauthenticated" && !disableAuth) {
    return <>{children}</>;
  }

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
        <div className="text-error font-semibold text-lg">
          Failed to load user data
        </div>
        <button onClick={refreshUser} className="btn btn-primary">
          Retry
        </button>
      </div>
    );
  }

  return (
    <RBACContext.Provider value={{ user, setUser, refreshUser, session }}>
      {children}
    </RBACContext.Provider>
  );
}
