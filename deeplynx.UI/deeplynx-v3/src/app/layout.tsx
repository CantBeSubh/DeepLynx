import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "@/app/globals.css";
import LayoutShell from "./(home)/components/LayoutShell";
import { UserSessionProvider } from "./contexts/UserSessionProvider";
import { ProjectSessionProvider } from "./contexts/ProjectSessionProvider";

export const metadata: Metadata = {
  title: "DeepLynx",
  description: "Container for DeepLynx app",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="bg-base-100">
      <body className="min-h-screed text-base-content">
        <UserSessionProvider>
          <ProjectSessionProvider>
            <LayoutShell>{children}</LayoutShell>
          </ProjectSessionProvider>
        </UserSessionProvider>
      </body>
    </html>
  );
}
