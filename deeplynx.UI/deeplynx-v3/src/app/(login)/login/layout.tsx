import { ProjectSessionProvider } from "@/app/contexts/ProjectSessionContext";
import { UserSessionProvider } from "@/app/contexts/UserSessionContext";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>
        <UserSessionProvider>
          <ProjectSessionProvider>{children}</ProjectSessionProvider>
        </UserSessionProvider>
      </body>
    </html>
  );
}
