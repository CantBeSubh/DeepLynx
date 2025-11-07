// src/app/(home)/components/LayoutShell.tsx
"use client";

import { useLanguage } from "@/app/contexts/Language";
import {
  ArrowRightStartOnRectangleIcon,
  BookOpenIcon,
  Cog6ToothIcon,
  GlobeAmericasIcon,
  UserCircleIcon,
} from "@heroicons/react/24/outline";
import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import React, { useState } from "react";
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

  const [selectedItem, setSelectedItem] = useState<string>("");

  // Handle menu toggle
  const [isMenuCollapsed, setIsMenuCollapsed] = React.useState(false);

  // Check if auth is disabled
  const isAuthDisabled =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

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
    return [firstName, lastName].filter(Boolean).join(" ");
  };

  // When auth is disabled, use RBAC user. When enabled, use session.
  const displayName = isAuthDisabled
    ? user?.name || ""
    : session?.user?.name || "";

  const displayEmail = isAuthDisabled
    ? user?.email || ""
    : session?.user?.email || "";

  const displayImage = session?.user?.image;

  // Function to handle item click events
  const handleItemClick = (
    item: string,
    event: React.MouseEvent<HTMLElement>
  ) => {
    event.preventDefault();
    setSelectedItem(item); // Set the clicked item as selected
    router.push(item); // Navigate to the clicked item's path
  };

  return (
    <div className="flex flex-col min-h-screen bg-base-100 text-base-content">
      {/* Banner/Header */}
      <header className="bg-base-300 text-primary-content flex justify-between items-center px-6 py-1 z-50 fixed w-full">
        <Image
          src="/images/lynx-white.png"
          alt="Logo"
          height={20}
          width={150}
          className="rounded cursor-pointer"
          onClick={() => router.push("/")}
        />
        <div className="dropdown dropdown-end">
          <div className="flex items-center">
            <RoleGate role="sysAdmin">
              <Link href={"/site_management"} prefetch={false}>
                <Cog6ToothIcon className="size-10" />
              </Link>
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
              <div className="flex bg-base-100">
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
                className="text-base-content hover:bg-base-200"
              >
                <Cog6ToothIcon className="size-6" />
                {t.translations.SETTINGS}
              </Link>
            </li>
            <li>
              <button
                className="text-base-content hover:bg-base-200"
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
        <div className="fixed top-18 bottom-0 flex">
          <aside
            className={
              "h-full shadow-xl w-18 bg-base-300 text-primary-content p-4 transition-all duration-300 flex flex-col"
            }
          >
            <ul className="mt-20">
              <li>
                <Link href={"#"}>
                  <GlobeAmericasIcon className="size-10" />
                </Link>
              </li>
              <li className="mt-5">
                <Link href={"#"}>
                  <Cog6ToothIcon className="size-10" />
                </Link>
              </li>
              <li className="mt-5">
                <Link
                  href="/data_catalog"
                  onClick={(e) => handleItemClick("/data_catalog", e)}
                >
                  <BookOpenIcon className="size-10" />
                </Link>
              </li>
            </ul>
          </aside>
        </div>
        <SideMenu onToggle={handleMenuToggle} />
        <main
          className={`transition-all duration-300 w-full mt-18 ${
            isMenuCollapsed ? "ml-40" : "ml-82"
          }`}
        >
          {children}
        </main>
      </div>
    </div>
  );
};

export default LayoutShell;
