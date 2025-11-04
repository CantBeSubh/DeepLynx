// src/app/(home)/components/LayoutShell.tsx

"use client";

import { useLanguage } from "@/app/contexts/Language";
import {
  ArrowRightStartOnRectangleIcon,
  Cog6ToothIcon,
  UserCircleIcon,
} from "@heroicons/react/24/outline";
import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import React from "react";
import SideMenu from "./SideMenu";
import AvatarCell from "./Avatar";
import { useSession, signOut } from "next-auth/react";
import { RoleGate } from "../rbac/RBACComponents";
import { useRBAC } from "../rbac/useRBAC";

const LayoutShell: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { t } = useLanguage();
  const router = useRouter();
  const { data: session } = useSession();
  const { user } = useRBAC();

  // Handle menu toggle
  const [isMenuCollapsed, setIsMenuCollapsed] = React.useState(false);

  const handleMenuToggle = (isCollapsed: boolean) => {
    setIsMenuCollapsed(isCollapsed);
  };

  const handleLogout = async () => {
    try {
      await signOut({
        callbackUrl: "/login/signin",
        redirect: true,
      });
    } catch (error) {
      console.error("Logout error:", error);
    }
  };

  const formatUserName = (fullName?: string | null): string => {
    if (!fullName) return "";

    const parts = fullName.trim().split(/\s+/);
    const firstName = parts[0] ?? "";
    const lastName = parts[parts.length - 1] ?? "";
    return [firstName, lastName].filter(Boolean).join(", ");
  };

  // Use RBAC user data, fallback to session data
  const displayName = user?.name || session?.user?.name || "";
  const displayEmail = user?.email || session?.user?.email || "";
  const displayImage = session?.user?.image;

  return (
    <div className="flex flex-col min-h-screen bg-base-100 text-base-content">
      {/* Banner/Header */}
      <header className="bg-primary text-primary-content flex justify-between items-center px-6 py-1 z-50 fixed w-full shadow-xl">
        <Image
          src="/images/lynx-white.png"
          alt="Logo"
          height={20}
          width={150}
          className="rounded cursor-pointer"
          onClick={() => router.push("/")}
        />
        <div className="dropdown dropdown-end">
          <div className="flex">
            <RoleGate role="sysAdmin">
              <div tabIndex={0} role="button" className="btn btn-ghost m-1">
                <Cog6ToothIcon className="size-10" />
              </div>
            </RoleGate>
            <div tabIndex={0} role="button" className="btn btn-ghost m-1">
              <UserCircleIcon className="size-10" />
            </div>
          </div>
          <ul
            tabIndex={0}
            className="menu dropdown-content bg-base-100 text-base-content rounded-box z-[100] w-auto min-w-52 max-w-[90vw] p-2 shadow-xl border border-base-300"
          >
            <li>
              <div className="flex hover:bg-base-300">
                <AvatarCell
                  image={displayImage ?? undefined}
                  name={displayName}
                  size={20}
                />
                <div className="flex-1 min-w-0">
                  <h1 className="font-bold text-lg text-base-content">
                    {formatUserName(displayName)}
                  </h1>
                  <p className="text-base-content/70 text-sm">{displayEmail}</p>
                </div>
              </div>
            </li>
            <li className="mt-2">
              <Link
                href="/settings"
                className="text-base-content hover:bg-base-300"
              >
                <Cog6ToothIcon className="size-6" />
                {t.translations.SETTINGS}
              </Link>
            </li>
            <li>
              <button
                className="text-base-content hover:bg-base-300"
                onClick={handleLogout}
              >
                <ArrowRightStartOnRectangleIcon className="size-6" />
                {t.translations.LOGOUT}
              </button>
            </li>
          </ul>
        </div>
      </header>

      {/* Page Content */}
      <div className="flex h-full z-0">
        {/* Side Menu */}
        <SideMenu onToggle={handleMenuToggle} />
        <main
          className={`transition-all duration-300 w-full mt-18 ${
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
