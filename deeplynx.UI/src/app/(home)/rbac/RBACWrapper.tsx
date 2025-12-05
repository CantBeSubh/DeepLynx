// src/app/(home)/rbac/RBACWrapper.tsx
"use client";

import React from "react";
import { RBACProvider } from "./RBACContext";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";

type Props = {
  children: React.ReactNode;
};

const normalizeId = (
  value: string | number | undefined | null
): number | undefined => {
  if (value === undefined || value === null) return undefined;
  const num = Number(value);
  return Number.isFinite(num) ? num : undefined;
};

export function RBACWrapper({ children }: Props) {
  const { organization } = useOrganizationSession();
  const { project } = useProjectSession();

  const orgId = normalizeId(organization?.organizationId);
  const projectId = normalizeId(project?.projectId);

  return (
    <RBACProvider orgId={orgId} projectId={projectId}>
      {children}
    </RBACProvider>
  );
}
