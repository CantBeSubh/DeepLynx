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
import { useSession } from "next-auth/react";

const LayoutShell: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { t } = useLanguage();
  const router = useRouter();
  const { data: session } = useSession();
  // Handle menu togle
  const [isMenuCollapsed, setIsMenuCollapsed] = React.useState(false);

  const handleMenuToggle = (isCollapsed: boolean) => {
    setIsMenuCollapsed(isCollapsed);
  };

  const formatUserName = (fullName?: string) => {
    if (!fullName) return "";

    const parts = fullName.split(" ");
    const firstName = parts[0];
    const lastName = parts[parts.length - 1];
    return `${firstName}, ${lastName}`;
  };

  return (
    <div className="flex flex-col min-h-screen bg-base-100 text-base-content">
      {/* Banner/Header */}
      <header className="bg-primary text-white flex justify-between items-center px-6 py-1 z-50 fixed w-full shadow-xl">
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
        <div className="dropdown dropdown-end">
          <div tabIndex={0} role="button" className="btn btn-ghost m-1">
            <UserCircleIcon className="size-10" />
          </div>
          <ul
            tabIndex={0}
            className="menu dropdown-content bg-base-100 rounded-box z-1 w-auto min-w-52 max-w-[90vw] p-2 shadow-sm"
          >
            <li>
              <div className="flex">
                <AvatarCell
                  image={session?.user?.image!}
                  name={session?.user?.name!}
                  size={20}
                />
                <div className="text-black flex-1 min-w-0">
                  <h1 className="font-bold text-lg">
                    {formatUserName(session?.user?.name!)}
                  </h1>
                  <p>{session?.user?.email}</p>
                </div>
              </div>
            </li>
            <li className="mt-2">
              <Link href="/settings" className="text-black">
                <Cog6ToothIcon className="size-6" />
                {t.translations.SETTINGS}
              </Link>
            </li>
            <li>
              <button className="text-black">
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
          className={`transition-all duration-300 w-full mt-18  ${
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
