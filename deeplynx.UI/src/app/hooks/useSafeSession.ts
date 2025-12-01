// src/app/hooks/useSafeSession.ts
"use client";

import { useSession } from "next-auth/react";
import type { Session } from "next-auth";
import { useEffect, useState } from "react";
import { getLocalDevUser } from "@/app/lib/client_service/user_services.client";

export function useSafeSession() {
  const isAuthDisabled =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  // Always call useSession (required by Rules of Hooks)
  const session = useSession();

  const [localSession, setLocalSession] = useState<Session | null>(null);
  const [isLoading, setIsLoading] = useState(isAuthDisabled);

  useEffect(() => {
    if (!isAuthDisabled) return;

    // Fetch the local dev user
    const fetchLocalUser = async () => {
      try {
        const localUser = await getLocalDevUser();

        // Create a session object that matches NextAuth's Session type
        const mockSession: Session = {
          user: {
            id: localUser.id?.toString() || "local-dev-user",
            name: localUser.name,
            email: localUser.email,
            image: undefined,
          },
          expires: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
        };

        setLocalSession(mockSession);
      } catch (error) {
        console.error("Error fetching local dev user:", error);

        // Fallback to basic mock session if fetch fails
        setLocalSession({
          user: {
            id: "local-dev-user",
            name: "Local Dev User",
            email: "dev@localhost",
            image: undefined,
          },
          expires: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
        });
      } finally {
        setIsLoading(false);
      }
    };

    fetchLocalUser();
  }, [isAuthDisabled]);

  // Return local session if auth is disabled
  if (isAuthDisabled) {
    return {
      data: localSession,
      status: isLoading ? ("loading" as const) : ("authenticated" as const),
      update: async () => localSession,
    };
  }

  // Otherwise return real session
  return session;
}