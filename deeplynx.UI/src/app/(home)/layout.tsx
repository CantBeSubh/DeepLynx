// src/app/(home)/layout.tsx
import React from "react";
import LayoutShell from "../(home)/components/LayoutShell";

// If you have client providers, wrap them here (but they must not suspend)
import { ProjectSessionProvider } from "../contexts/ProjectSessionProvider";
import AuthGuard from "./components/AuthGuard";
import { RBACProvider } from "./rbac/RBACContext";
import { OrganizationSessionProvider } from "../contexts/OrganizationSessionProvider";
import { RBACWrapper } from "./rbac/RBACWrapper";

export default function HomeLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthGuard>
      <OrganizationSessionProvider>
        <ProjectSessionProvider>
          <RBACWrapper>
            <LayoutShell>{children}</LayoutShell>
          </RBACWrapper>
        </ProjectSessionProvider>
      </OrganizationSessionProvider>
    </AuthGuard>
  );
}
