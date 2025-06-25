"use client";

import { useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useUserSession } from "@/app/contexts/UserSessionProvider";

export default function AuthCallback() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const { setUser } = useUserSession();

  useEffect(() => {
    const code = searchParams.get("code");

    if (code) {
      // TODO: Call your backend API to exchange code for a user token
      // Example: const user = await exchangeCodeForUser(code);

      const mockUser = {
        username: "deep_nexus",
        email: "nexus@example.com",
        isLoggedIn: true,
      };

      // Save in session
      setUser(mockUser);

      // Redirect to protected route
      router.push("/");
    } else {
      router.push("/login"); // fallback
    }
  }, [searchParams, setUser, router]);

  return <p className="p-8 text-center">Logging you in...</p>;
}
