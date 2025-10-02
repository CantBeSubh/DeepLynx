"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import React, { useEffect, useState } from "react";

// Importing Hero-UI icons
import { useLanguage } from "@/app/contexts/Language";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import {
  AdjustmentsHorizontalIcon,
  ArrowUpTrayIcon,
  BookmarkSquareIcon,
  BugAntIcon,
  ChatBubbleLeftRightIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  FolderIcon,
  PresentationChartLineIcon,
  QuestionMarkCircleIcon,
  RectangleGroupIcon,
} from "@heroicons/react/24/outline";

// Define the props for the SideMenu component
interface SideMenuProps {
  onToggle: (isCollapsed: boolean) => void;
}

// Main translations component
const SideMenu: React.FC<SideMenuProps> = ({ onToggle }) => {
  const { t } = useLanguage();
  const router = useRouter(); // Router hook for navigation
  const pathname = usePathname(); // Hook to get the current pathname
  const { project } = useProjectSession();

  // State variables for selected item and menu collapse state
  const [selectedItem, setSelectedItem] = useState<string>("");
  const [isCollapsed, setIsCollapsed] = useState<boolean>(false);

  // Effect to set the selected item based on the current pathname
  useEffect(() => {
    setSelectedItem(pathname);
  }, [pathname]);

  // Effect to get the selected item from localStorage on initial render
  useEffect(() => {
    const savedItem = localStorage.getItem("selectedItem");
    if (savedItem) setSelectedItem(savedItem);
  }, []);

  // Effect to save the selected item to localStorage whenever it changes
  useEffect(() => {
    localStorage.setItem("selectedItem", selectedItem);
  }, [selectedItem]);

  useEffect(() => {
    onToggle(isCollapsed);
  }, [isCollapsed, onToggle]);

  // Function to toggle the collapse state of the menu
  const toggleMenu = () => {
    const newState = !isCollapsed;
    setIsCollapsed(newState);
    onToggle(newState);
  };

  // Function to handle item click events
  const handleItemClick = (
    item: string,
    event: React.MouseEvent<HTMLElement>
  ) => {
    event.preventDefault();
    setSelectedItem(item); // Set the clicked item as selected
    router.push(item); // Navigate to the clicked item's path
  };

  // Function to check if an item is disabled
  const isDisabled = (targetPath: string) =>
    pathname === "/" && targetPath !== "/";

  // Function to get the CSS class for an item based on its state
  const getItemClass = (targetPath: string) => {
    const alwaysActivePaths = [
      "/settings",
      "/help",
      "/contact",
      "/fileBug",
      "/upload_center",
      "/data_catalog",
      "/member_management",
    ];
    const isExactMatch = selectedItem === targetPath;
    const isDynamicProject =
      targetPath === "/project/[id]" && /^\/project\/[^/]+$/.test(pathname);
    const isSelected = isExactMatch || isDynamicProject;
    const isAlwaysActive = alwaysActivePaths.includes(targetPath);

    return [
      "flex items-center block py-2 px-4 rounded transition",
      isSelected ? "bg-info/30" : "hover:bg-info/30",
      isDisabled(targetPath) && !isAlwaysActive
        ? "pointer-events-none opacity-50 cursor-not-allowed"
        : "",
    ].join(" ");
  };

  return (
    <div className="fixed top-18 bottom-0 flex z-50">
      <aside
        className={`h-full shadow-xl ${isCollapsed ? "w-22" : "w-64"
          } bg-secondary text-primary-content p-4 transition-all duration-300 flex flex-col`}
      >
        {/* Home */}
        <ul className="">
          <li>
            <Link
              href="/data_catalog"
              onClick={(e) => handleItemClick("/data_catalog", e)}
              className={getItemClass("/data_catalog")}
            >
              <FolderIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.DATA_CATALOG}</p>
              )}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href={"/upload_center"}
              className={getItemClass("/upload_center")}
            >
              <ArrowUpTrayIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.UPLOAD_CENTER}</p>
              )}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href={"/member_management"}
              onClick={(e) => handleItemClick("/member_management", e)}
              className={getItemClass("/member_management")}
            >
              <AdjustmentsHorizontalIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.MEMBER_MANAGEMENT}</p>
              )}
            </Link>
          </li>
        </ul>

        <div className="divider" />

        <ul className="flex-grow">
          <li>
            <Link
              href={`/project/${project?.projectId}`}
              prefetch={false}
              onClick={(e) =>
                handleItemClick(`/project/${project?.projectId}`, e)
              }
              className={getItemClass(`/project/${project?.projectId}`)}
            >
              <RectangleGroupIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.PROJECT_MANAGEMENT}</p>
              )}
            </Link>
          </li>

          <li className="mt-2">
            <Link
              href="#saved-searches"
              onClick={(e) => handleItemClick("#saved-searches", e)}
              className={getItemClass("#saved-searches")}
            >
              <BookmarkSquareIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.SAVED_SEARCHES}</p>
              )}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href="#timeseries-viewer"
              onClick={(e) => handleItemClick("#timeseries-viewer", e)}
              className={getItemClass("#timeseries-viewer")}
            >
              <PresentationChartLineIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.TIMESERIES_VIEWER}</p>
              )}
            </Link>
          </li>
          <li className="mt-2">
            <div className="flex items-center">
              <Link
                href="/project_settings"
                onClick={(e) => handleItemClick(`/project/${project?.projectId}/project_settings`, e)}
                className={getItemClass(`/project/${project?.projectId}/project_settings`)}
              >
                <AdjustmentsHorizontalIcon className="size-6" />
                {!isCollapsed && (
                  <p className="ml-2">{t.translations.PROJECT_SETINGS}</p>
                )}
              </Link>
            </div>
          </li>
        </ul>

        <div className="divider" />
        {/* Last 4 Menu Items */}
        {/* BUG ISSUE: When ever a project is not selected all middle menu items should be disabled. But the bug is, when the last 4 menu items are clicked it activates the middle menu items. */}
        <div className="mt-auto">
          <ul>
            <li className="mt-2">
              <Link
                href={process.env.NEXT_PUBLIC_DOCS_PATH ? `${process.env.NEXT_PUBLIC_DOCS_PATH}` : "http://localhost:3001"}
                /*
                href="#"
                prefetch={false}
                onClick={(e) => {
                  e.preventDefault();
                  // open modal / external
                }}
                // onClick={(e) => handleItemClick("/help", e)}
                */
                className={getItemClass("/help")}
              >
                <QuestionMarkCircleIcon className="size-6" />
                {!isCollapsed && <p className="ml-2">{t.translations.HELP}</p>}
              </Link>
            </li>
            <li className="mt-2">
              <Link
                href="#"
                prefetch={false}
                onClick={(e) => {
                  e.preventDefault();
                  // open modal / external
                }}
                // onClick={(e) => handleItemClick("/contact", e)}
                className={getItemClass("/contact")}
              >
                <ChatBubbleLeftRightIcon className="size-6" />
                {!isCollapsed && (
                  <p className="ml-2">{t.translations.CONTACT}</p>
                )}
              </Link>
            </li>
            <li className="mt-2">
              <Link
                href="#"
                prefetch={false}
                onClick={(e) => {
                  e.preventDefault();
                  // open modal / external
                }}
                // onClick={(e) => handleItemClick("/fileBug", e)}
                className={getItemClass("/fileBug")}
              >
                <BugAntIcon className="size-6" />
                {!isCollapsed && (
                  <p className="ml-2">{t.translations.FILE_A_BUG}</p>
                )}
              </Link>
            </li>
          </ul>
        </div>
      </aside>
      {/* Toggle tab (sticking out to the right) */}
      <div
        className="h-8 w-4 bg-secondary text-primary-content flex items-center justify-center cursor-pointer rounded-r-md mt-20"
        onClick={toggleMenu}
      >
        {isCollapsed ? (
          <ChevronRightIcon className="size-6" />
        ) : (
          <ChevronLeftIcon className="size-6" />
        )}
      </div>
    </div>
  );
};

export default SideMenu;
