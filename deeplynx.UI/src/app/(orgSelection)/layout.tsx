import { RBACProvider } from "../(home)/rbac/RBACContext";
import { OrganizationSessionProvider } from "../contexts/OrganizationSessionProvider";

// app/(selection)/layout.tsx
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
