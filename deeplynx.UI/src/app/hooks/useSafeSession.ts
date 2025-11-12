// src/app/hooks/useSafeSession.ts
"use client";

import { useSession } from "next-auth/react";

export function useSafeSession() {
  const isAuthDisabled =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  // Always call useSession (required by Rules of Hooks)
  const session = useSession();

  // Return fake session if auth is disabled
  if (isAuthDisabled) {
    return { data: null, status: "authenticated" as const };
  }

  return session;
}