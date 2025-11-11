import { RBACProvider } from "../(home)/rbac/RBACContext";
import { OrganizationSessionProvider } from "../contexts/OrganizationSessionProvider";
import { UserSessionProvider } from "../contexts/UserSessionProvider";

// app/(selection)/layout.tsx
export default function SelectionLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RBACProvider>
      <OrganizationSessionProvider>
        <UserSessionProvider>
          <div className="min-h-screen bg-base-200">{children}</div>
        </UserSessionProvider>
      </OrganizationSessionProvider>
    </RBACProvider>
  );
}
