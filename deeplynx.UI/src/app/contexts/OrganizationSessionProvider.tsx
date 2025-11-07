"use client";

import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
} from "react";
import type { ReactNode, Context } from "react";
import { OrganizationResponseDto } from "../(home)/types/responseDTOs";

// Define shape of an organization session (simplified from the full DTO)
export interface OrganizationSession {
  organizationId: string | number;
  organizationName: string;
}

// Define context value shape
interface OrganizationSessionContextType {
  organization: OrganizationSession | null;
  setOrganization: (
    organization: OrganizationSession | OrganizationResponseDto
  ) => void;
  hasLoaded: boolean;
}

// Create context with explicit typing
const OrganizationSessionContext: Context<
  OrganizationSessionContextType | undefined
> = createContext<OrganizationSessionContextType | undefined>(undefined);

// Custom hook for accessing the session
export const useOrganizationSession = (): OrganizationSessionContextType => {
  const context = useContext(OrganizationSessionContext);
  if (!context) {
    throw new Error(
      "useOrganizationSession must be used within an OrganizationSessionProvider"
    );
  }
  return context;
};

// Helper function to convert DTO to session
const toOrganizationSession = (
  org: OrganizationSession | OrganizationResponseDto
): OrganizationSession => {
  // If it's already in session format, return as-is
  if ("organizationId" in org && "organizationName" in org) {
    return org;
  }

  // Convert from DTO format
  return {
    organizationId: org.id,
    organizationName: org.name,
  };
};

// Provider component
export const OrganizationSessionProvider = ({
  children,
}: {
  children: ReactNode;
}): React.JSX.Element => {
  const [organization, setOrganizationState] =
    useState<OrganizationSession | null>(null);
  const [hasLoaded, setHasLoaded] = useState(false);

  // On mount, restore from localStorage
  useEffect(() => {
    const stored = localStorage.getItem("organizationSession");
    if (stored) {
      try {
        const parsed: OrganizationSession = JSON.parse(stored);
        setOrganizationState(parsed);
      } catch {
        console.warn("Failed to parse stored organization session.");
      }
    }
    setHasLoaded(true);
  }, []);

  // On organization update, persist to localStorage
  const setOrganization = useCallback(
    (org: OrganizationSession | OrganizationResponseDto) => {
      const session = toOrganizationSession(org);
      setOrganizationState(session);
      localStorage.setItem("organizationSession", JSON.stringify(session));
    },
    []
  );

  return (
    <OrganizationSessionContext.Provider
      value={{ organization, setOrganization, hasLoaded }}
    >
      {children}
    </OrganizationSessionContext.Provider>
  );
};
