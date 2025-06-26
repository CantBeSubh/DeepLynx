"use client";

import { useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useUserSession } from "@/app/contexts/UserSessionProvider";

export default function AuthCallbackClient() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const { setUser } = useUserSession();

  useEffect(() => {
    const code = searchParams.get("code");

    if (code) {
      const mockUser = {
        username: "deep_nexus",
        email: "nexus@example.com",
        isLoggedIn: true,
      };

      setUser(mockUser);
      router.push("/");
    } else {
      router.push("/login");
    }
  }, [searchParams, setUser, router]);

  return null;
}
