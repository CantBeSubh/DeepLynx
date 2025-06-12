import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import LayoutShell from "./components/LayoutShell";

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
        <LayoutShell>{children}</LayoutShell>
      </body>
    </html>
  );
}
