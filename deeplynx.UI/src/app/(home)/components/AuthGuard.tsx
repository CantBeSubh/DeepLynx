// src/app/components/AuthGuard.tsx
"use client";

import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

interface AuthGuardProps {
  children: React.ReactNode;
  redirectTo?: string;
}

export default function AuthGuard({
  children,
  redirectTo = "/login/signin",
}: AuthGuardProps) {
  const { data: session, status } = useSession();
  const router = useRouter();

  useEffect(() => {
    // If loading, don't do anything yet
    if (status === "loading") return;

    // If not authenticated, redirect to login
    if (status === "unauthenticated" || !session) {
      router.push(redirectTo);
      return;
    }
  }, [status, session, router, redirectTo]);

  // Show loading while checking authentication
  if (status === "loading") {
    return (
      <div className="min-h-screen flex items-center justify-center bg-base-100">
        <div className="text-center">
          <div className="loading loading-spinner loading-lg"></div>
          <p className="mt-4">Checking authentication...</p>
        </div>
      </div>
    );
  }

  // Show nothing while redirecting unauthenticated users
  if (status === "unauthenticated" || !session) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-base-100">
        <div className="text-center">
          <div className="loading loading-spinner loading-lg"></div>
          <p className="mt-4">Redirecting to login...</p>
        </div>
      </div>
    );
  }

  // User is authenticated, show the protected content
  // Local development bypass
  const disableAuth = process.env.DISABLE_FRONTEND_AUTHENTICATION;

  if (disableAuth == "true") {
    console.log(disableAuth);
    return <>{children}</>;
  } else return <>{children}</>;
}
