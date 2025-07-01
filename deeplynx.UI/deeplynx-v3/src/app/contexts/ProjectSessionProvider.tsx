"use client";

import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  ReactNode,
  useCallback,
} from "react";

// Define shape of a project session
export interface ProjectSession {
  projectId: string;
  projectName: string;
}

// Define context value shape
interface ProjectSessionContextType {
  project: ProjectSession | null;
  setProject: (project: ProjectSession) => void;
  hasLoaded: boolean;
}

// Create context
const ProjectSessionContext = createContext<
  ProjectSessionContextType | undefined
>(undefined);

// Custom hook for accessing the session
export const useProjectSession = () => {
  const context = useContext(ProjectSessionContext);
  if (!context) {
    throw new Error(
      "useProjectSession must be used within a ProjectSessionProvider"
    );
  }
  return context;
};

// Provider component
export const ProjectSessionProvider = ({
  children,
}: {
  children: ReactNode;
}) => {
  const [project, setProjectState] = useState<ProjectSession | null>(null);
  const [hasLoaded, setHasLoaded] = useState(false);

  // On mount, restore from localStorage
  useEffect(() => {
    const stored = localStorage.getItem("projectSession");
    if (stored) {
      try {
        const parsed: ProjectSession = JSON.parse(stored);
        setProjectState(parsed);
      } catch {
        console.warn("Failed to parse stored project session.");
      }
    }
    setHasLoaded(true);
  }, []);

  // On project update, persist to localStorage
  const setProject = useCallback((project: ProjectSession) => {
    setProjectState(project);
    localStorage.setItem("projectSession", JSON.stringify(project));
  }, []);

  return (
    <ProjectSessionContext.Provider value={{ project, setProject, hasLoaded }}>
      {children}
    </ProjectSessionContext.Provider>
  );
};
