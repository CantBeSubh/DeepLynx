// src/app/(orgSelection)/layout.tsx
import { RBACProvider } from "../(home)/rbac/RBACContext";
import { OrganizationSessionProvider } from "../contexts/OrganizationSessionProvider";

export default function SelectionLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RBACProvider>
      <OrganizationSessionProvider>
        <div className="min-h-screen bg-base-200">{children}</div>
      </OrganizationSessionProvider>
    </RBACProvider>
  );
}
