// src/app/(home)/components/AuthGuard.tsx
"use client";

import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import Image from "next/image";

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

  // Check if auth is disabled
  const disableAuth =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  useEffect(() => {
    // Skip auth check if disabled
    if (disableAuth) {
      return;
    }

    // If loading, don't do anything yet
    if (status === "loading") return;

    // If not authenticated, redirect to login
    if (status === "unauthenticated" || !session) {
      router.push(redirectTo);
      return;
    }
  }, [status, session, router, redirectTo, disableAuth]);

  // If auth is disabled, always render children immediately
  if (disableAuth) {
    return <>{children}</>;
  }

  // Show loading while checking authentication
  if (status === "loading") {
    return (
      <div className="min-h-screen flex items-center justify-center login">
        <div className="text-center">
          <div className="loading loading-spinner loading-lg"></div>
          <Image
            src="/assets/nexusWhite.png"
            alt="DeepLynx logo"
            width={265.8}
            height={113.9}
            priority
          />
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
  return <>{children}</>;
}
