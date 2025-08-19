// app/(home)/layout.tsx
import React, { Suspense } from "react";
import LayoutShell from "../(home)/components/LayoutShell";

// If you have client providers, wrap them here (but they must not suspend)
import { UserSessionProvider } from "../contexts/UserSessionProvider";
import { ProjectSessionProvider } from "../contexts/ProjectSessionProvider";

export default function HomeLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <UserSessionProvider>
      <ProjectSessionProvider>
        <LayoutShell>{children}</LayoutShell>
      </ProjectSessionProvider>
    </UserSessionProvider>
  );
}
