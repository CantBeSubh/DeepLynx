// src/app/(home)/components/AuthGuard.tsx
"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import Image from "next/image";
import { useSafeSession } from "@/app/hooks/useSafeSession";

interface AuthGuardProps {
  children: React.ReactNode;
  redirectTo?: string;
}

export default function AuthGuard({
  children,
  redirectTo = "/login/signin",
}: AuthGuardProps) {
  const router = useRouter();

  const disableAuth =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  const { data: session, status } = useSafeSession();

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

  // Show loading while checking authentication OR fetching local user
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

  // If auth is disabled, wait for local session to be loaded
  if (disableAuth && !session) {
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
          <p className="mt-4 text-white">Loading local user...</p>
        </div>
      </div>
    );
  }

  // Show nothing while redirecting unauthenticated users
  if (!disableAuth && (status === "unauthenticated" || !session)) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-base-100">
        <div className="text-center">
          <div className="loading loading-spinner loading-lg"></div>
          <p className="mt-4 text-base-content">Redirecting to login...</p>
        </div>
      </div>
    );
  }

  // User is authenticated (or local session is loaded), show the protected content
  return <>{children}</>;
}