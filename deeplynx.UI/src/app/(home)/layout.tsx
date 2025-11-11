// src/app/(home)/layout.tsx
import React from "react";
import LayoutShell from "../(home)/components/LayoutShell";
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
