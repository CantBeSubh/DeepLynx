"use client";

import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  ReactNode,
} from "react";

// Define the shape of the user session
interface UserSession {
  isLoggedIn: boolean;
  username: string;
  // Add more fields as needed
}

// Define the shape of the context value
interface UserSessionContextType {
  user: UserSession;
  setUser: React.Dispatch<React.SetStateAction<UserSession>>;
}

// Create the context
const UserSessionContext = createContext<UserSessionContextType | undefined>(
  undefined
);

// Custom hook for accessing the user session
export const useUserSession = () => {
  const context = useContext(UserSessionContext);
  if (!context) {
    throw new Error("useUserSession must be used within a UserSessionProvider");
  }
  return context;
};

// Provider component
export const UserSessionProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserSession>(() => {
    // Try to restore session from localStorage on first render
    if (typeof window !== "undefined") {
      const stored = localStorage.getItem("userSession");
      return stored ? JSON.parse(stored) : { isLoggedIn: false, username: "" };
    }
    return { isLoggedIn: false, username: "" };
  });

  useEffect(() => {
    localStorage.setItem("userSession", JSON.stringify(user));
  }, [user]);

  return (
    <UserSessionContext.Provider value={{ user, setUser }}>
      {children}
    </UserSessionContext.Provider>
  );
};
