"use client";

import { UserCircleIcon } from "@heroicons/react/24/outline";
import Image from "next/image";
import React from "react";
import SideMenu from "./SideMenu";
import { useRouter } from "next/navigation";

const LayoutShell: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const router = useRouter();
  // Handle menu togle
  const [isMenuCollapsed, setIsMenuCollapsed] = React.useState(false);

  const handleMenuToggle = (isCollapsed: boolean) => {
    setIsMenuCollapsed(isCollapsed);
  };

  return (
    <div className="flex flex-col min-h-screen bg-base-100 text-base-content">
      {/* Banner/Header */}
      <header className="bg-primary text-white flex justify-between items-center px-6 py-1 shadow-md z-50 fixed w-full">
        <Image
          src="/images/lynx-white.png"
          alt="Logo"
          height={20}
          width={150}
          className="rounded cursor-pointer"
          style={{
            WebkitMaskImage:
              "linear-gradient(to right, transparent, black 5%, black 95%, transparent)",
            maskImage:
              "linear-gradient(to right, transparent, black 5%, black 95%, transparent)",
            WebkitMaskRepeat: "no-repeat",
            maskRepeat: "no-repeat",
          }}
          onClick={() => router.push("/")}
        />
        {/* <label className="toggle text-neutral">
          <input type="checkbox" value="dark" className="theme-controller" />

          <SunIcon className="size-4" />

          <MoonIcon className="size-4" />
        </label> */}
        <div className="dropdown">
          <div tabIndex={0} role="button" className="btn btn-ghost m-1">
            <UserCircleIcon className="size-10" />
          </div>
          <ul
            tabIndex={0}
            className="menu dropdown-content bg-base-100 rounded-box z-1 w-52 p-2 shadow-sm"
          >
            <li>
              <a className="text-secondary-content">Item 1</a>
            </li>
            <li>
              <button className="text-secondary-content">Logout</button>
            </li>
          </ul>
        </div>
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
