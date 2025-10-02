// src/app/(home)/layout.tsx
import React, { Suspense } from "react";
import LayoutShell from "../(home)/components/LayoutShell";

// If you have client providers, wrap them here (but they must not suspend)
import { UserSessionProvider } from "../contexts/UserSessionProvider";
import { ProjectSessionProvider } from "../contexts/ProjectSessionProvider";
import {NotificationProvider} from "../contexts/NotificationProvider";
import AuthGuard from "./components/AuthGuard";

export default function HomeLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const disableAuth = process.env.DISABLE_FRONTEND_AUTHENTICATION;

  if (disableAuth == "true") {
    return (
      <UserSessionProvider>
        <ProjectSessionProvider>
          <NotificationProvider>
            <LayoutShell>{children}</LayoutShell>
          </NotificationProvider>    
        </ProjectSessionProvider>
      </UserSessionProvider>
    )
  } else
    return (
      <UserSessionProvider>
        <AuthGuard>
          <ProjectSessionProvider>
            <NotificationProvider>
                <LayoutShell>{children}</LayoutShell>
            </NotificationProvider>    
          </ProjectSessionProvider>
        </AuthGuard>
      </UserSessionProvider>
    );
}
