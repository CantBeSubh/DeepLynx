// src/app/(home)/layout.tsx
import React, { Suspense } from "react";
import LayoutShell from "../(home)/components/LayoutShell";

// If you have client providers, wrap them here (but they must not suspend)
import { ProjectSessionProvider } from "../contexts/ProjectSessionProvider";
import AuthGuard from "./components/AuthGuard";
import { RBACProvider } from "./rbac/RBACContext";

export default function HomeLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthGuard>
      <RBACProvider>
        <ProjectSessionProvider>
          <LayoutShell>{children}</LayoutShell>
        </ProjectSessionProvider>
      </RBACProvider>
    </AuthGuard>
  );
}
