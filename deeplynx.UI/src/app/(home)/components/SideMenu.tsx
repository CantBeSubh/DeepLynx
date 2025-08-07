"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import React, { useEffect, useState } from "react";

// Importing Hero-UI icons
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import {
  ArrowsPointingOutIcon,
  BookmarkSquareIcon,
  BugAntIcon,
  CalendarDaysIcon,
  ChatBubbleLeftRightIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  Cog6ToothIcon,
  DocumentIcon,
  HomeIcon,
  InboxIcon,
  ListBulletIcon,
  PresentationChartLineIcon,
  QuestionMarkCircleIcon,
} from "@heroicons/react/24/outline";

// Define the props for the SideMenu component
interface SideMenuProps {
  onToggle: (isCollapsed: boolean) => void;
}

// Main SideMenu component
const SideMenu: React.FC<SideMenuProps> = ({ onToggle }) => {
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
    const alwaysActivePaths = ["/settings", "/help", "/contact", "/fileBug"];
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
    <aside
      className={`fixed ${
        isCollapsed ? "w-22" : "w-64"
      } bg-secondary text-primary-content top-16 bottom-0 p-4 transition-width duration-300 flex flex-col`}
    >
      {/* Button to toggle menu collapse state */}
      <div className="flex justify-end">
        <button onClick={toggleMenu} className="p-2">
          {isCollapsed ? (
            <ChevronRightIcon className="size-6" />
          ) : (
            <ChevronLeftIcon className="size-6" />
          )}
        </button>
      </div>

      {/* Home */}
      <ul>
        <li>
          <Link
            href="/"
            onClick={(e) => handleItemClick("/", e)}
            className={getItemClass("/")}
          >
            <HomeIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Home Dashboard</p>}
          </Link>
        </li>
      </ul>

      <div className="divider" />

      {/* Your Data section */}
      {/* {!isCollapsed && <p className="text-sm mt-4">Your Data</p>} */}
      <ul className="flex-grow">
        <li>
          <Link
            href="/project/#"
            onClick={(e) =>
              handleItemClick(`/project/${project?.projectId}`, e)
            }
            className={getItemClass(`/project/${project?.projectId}`)}
          >
            <ListBulletIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Project Management</p>}
          </Link>
        </li>
        <li className="mt-2">
          <Link
            href="#ontology"
            onClick={(e) => handleItemClick("#ontology", e)}
            className={getItemClass("#ontology")}
          >
            <ArrowsPointingOutIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Ontology</p>}
          </Link>
        </li>
        <li className="mt-2">
          <Link
            href="/data_source"
            onClick={(e) => handleItemClick("/data_source", e)}
            className={getItemClass("/data_source")}
          >
            <InboxIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Data Source</p>}
          </Link>
        </li>
        <li className="mt-2">
          <Link
            href="/data_catalog"
            onClick={(e) => handleItemClick("/data_catalog", e)}
            className={getItemClass("/data_catalog")}
          >
            <InboxIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Data Catalog</p>}
          </Link>
        </li>
        <li className="mt-2">
          <Link
            href="#saved-searches"
            onClick={(e) => handleItemClick("#saved-searches", e)}
            className={getItemClass("#saved-searches")}
          >
            <BookmarkSquareIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Saved Searches</p>}
          </Link>
        </li>
        <li className="mt-2">
          <Link
            href="#events"
            onClick={(e) => handleItemClick("#events", e)}
            className={getItemClass("#events")}
          >
            <CalendarDaysIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Events</p>}
          </Link>
        </li>
        <li className="mt-2">
          <Link
            href="#timeseries-viewer"
            onClick={(e) => handleItemClick("#timeseries-viewer", e)}
            className={getItemClass("#timeseries-viewer")}
          >
            <PresentationChartLineIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">Timeseries Viewer</p>}
          </Link>
        </li>
        <li className="mt-2">
          <Link
            href="/file_viewer"
            onClick={(e) => handleItemClick("/file_viewer", e)}
            className={getItemClass("/file_viewer")}
          >
            <DocumentIcon className="size-6" />
            {!isCollapsed && <p className="ml-2">File Viewer</p>}
          </Link>
        </li>
      </ul>

      <div className="divider" />
      {/* Last 4 Menu Items */}
      {/* BUG ISSUE: When ever a project is not selected all middle menu items should be disabled. But the bug is, when the last 4 menu items are clicked it activates the middle menu items. */}
      <div className="mt-auto">
        <ul>
          <li className="mt-2">
            <Link
              href="/settings"
              onClick={(e) => handleItemClick("/settings", e)}
              className={getItemClass("/settings")}
            >
              <Cog6ToothIcon className="size-6" />
              {!isCollapsed && <p className="ml-2">Settings</p>}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href="#"
              onClick={(e) => handleItemClick("/help", e)}
              className={getItemClass("/help")}
            >
              <QuestionMarkCircleIcon className="size-6" />
              {!isCollapsed && <p className="ml-2">Help</p>}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href="/contact"
              onClick={(e) => handleItemClick("/contact", e)}
              className={getItemClass("/contact")}
            >
              <ChatBubbleLeftRightIcon className="size-6" />
              {!isCollapsed && <p className="ml-2">Contact</p>}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href="/fileBug"
              onClick={(e) => handleItemClick("/fileBug", e)}
              className={getItemClass("/fileBug")}
            >
              <BugAntIcon className="size-6" />
              {!isCollapsed && <p className="ml-2">File A Bug</p>}
            </Link>
          </li>
        </ul>
      </div>
    </aside>
  );
};

export default SideMenu;
