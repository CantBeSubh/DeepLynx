"use client";

import React from "react";
import Image from "next/image";
import SideMenu from "./SideMenu";

const LayoutShell: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // Handle menu togle
  const [isMenuCollapsed, setIsMenuCollapsed] = React.useState(false);

  const handleMenuToggle = (isCollapsed: boolean) => {
    setIsMenuCollapsed(isCollapsed);
  };

  return (
    <div className="flex flex-col bg-base-100">
      {/* Banner/Header */}
      <header className="bg-primary text-white flex justify-between items-center px-6 py-1 shadow-md z-50 fixed w-full">
        <Image
          src="/images/lynx.png"
          alt="Logo"
          height={20}
          width={150}
          className="rounded"
          style={{
            WebkitMaskImage:
              "linear-gradient(to right, transparent, black 5%, black 95%, transparent)",
            maskImage:
              "linear-gradient(to right, transparent, black 5%, black 95%, transparent)",
            WebkitMaskRepeat: "no-repeat",
            maskRepeat: "no-repeat",
          }}
        />
        <label className="toggle text-neutral">
          <input type="checkbox" value="dark" className="theme-controller" />

          <svg
            aria-label="sun"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
          >
            <g
              strokeLinejoin="round"
              strokeLinecap="round"
              strokeWidth="2"
              fill="none"
              stroke="currentColor"
            >
              <circle cx="12" cy="12" r="4"></circle>
              <path d="M12 2v2"></path>
              <path d="M12 20v2"></path>
              <path d="m4.93 4.93 1.41 1.41"></path>
              <path d="m17.66 17.66 1.41 1.41"></path>
              <path d="M2 12h2"></path>
              <path d="M20 12h2"></path>
              <path d="m6.34 17.66-1.41 1.41"></path>
              <path d="m19.07 4.93-1.41 1.41"></path>
            </g>
          </svg>

          <svg
            aria-label="moon"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
          >
            <g
              strokeLinejoin="round"
              strokeLinecap="round"
              strokeWidth="2"
              fill="none"
              stroke="currentColor"
            >
              <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z"></path>
            </g>
          </svg>
        </label>
        <div className="text-sm">User</div>
      </header>

      {/* Page Content */}
      <div className="flex h-full">
        {/* Side Menu */}
        <SideMenu onToggle={handleMenuToggle} />
        <main
          className={`transition-all duration-300 w-full p-6 mt-20  ${
            isMenuCollapsed ? "ml-20" : "ml-64"
          }`}
        >
          {children}
        </main>
      </div>
    </div>
  );
};

export default LayoutShell;
