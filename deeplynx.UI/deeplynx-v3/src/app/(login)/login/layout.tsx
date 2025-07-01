import { ProjectSessionProvider } from "@/app/contexts/ProjectSessionProvider";
import { UserSessionProvider } from "@/app/contexts/UserSessionProvider";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
