// src/app/(home)/layout.tsx
import React, { Suspense } from "react";
import LayoutShell from "../(home)/components/LayoutShell";

// If you have client providers, wrap them here (but they must not suspend)
import { UserSessionProvider } from "../contexts/UserSessionProvider";
import { ProjectSessionProvider } from "../contexts/ProjectSessionProvider";
import AuthGuard from "./components/AuthGuard";
import { RBACProvider } from "./rbac/RBACContext";
import { OrganizationSessionProvider } from "../contexts/OrganizationSessionProvider";

export default function HomeLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthGuard>
      <RBACProvider>
        <OrganizationSessionProvider>
          <ProjectSessionProvider>
            <LayoutShell>{children}</LayoutShell>
          </ProjectSessionProvider>
        </OrganizationSessionProvider>
      </RBACProvider>
    </AuthGuard>
  );
}
