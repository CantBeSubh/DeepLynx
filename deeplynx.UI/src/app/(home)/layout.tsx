// src/app/(home)/layout.tsx
import React, { Suspense } from "react";
import LayoutShell from "../(home)/components/LayoutShell";

// If you have client providers, wrap them here (but they must not suspend)
import { UserSessionProvider } from "../contexts/UserSessionProvider";
import { ProjectSessionProvider } from "../contexts/ProjectSessionProvider";
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
                    <LayoutShell>{children}</LayoutShell>
                </ProjectSessionProvider>
            </UserSessionProvider>
        )
    } else
        return (
            <UserSessionProvider>
                <AuthGuard>
                    <ProjectSessionProvider>
                        <LayoutShell>{children}</LayoutShell>
                    </ProjectSessionProvider>
                </AuthGuard>
            </UserSessionProvider>
        );
}