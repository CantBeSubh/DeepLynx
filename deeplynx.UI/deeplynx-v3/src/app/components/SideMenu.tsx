"use client";

import React, { useState, useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import Link from "next/link";

// Importing Material-UI icons
import OtherHousesOutlinedIcon from "@mui/icons-material/OtherHousesOutlined";
import ListAltOutlinedIcon from "@mui/icons-material/ListAltOutlined";
import ManageSearchIcon from "@mui/icons-material/ManageSearch";
import TimelineIcon from "@mui/icons-material/Timeline";
import FindInPageOutlinedIcon from "@mui/icons-material/FindInPageOutlined";
import CoronavirusOutlinedIcon from "@mui/icons-material/CoronavirusOutlined";
import InboxIcon from "@mui/icons-material/Inbox";
import SellOutlinedIcon from "@mui/icons-material/SellOutlined";
import ViewInArOutlinedIcon from "@mui/icons-material/ViewInArOutlined";
import CalendarMonthOutlinedIcon from "@mui/icons-material/CalendarMonthOutlined";
import KeyboardArrowLeftTwoToneIcon from "@mui/icons-material/KeyboardArrowLeftTwoTone";
import KeyboardArrowRightTwoToneIcon from "@mui/icons-material/KeyboardArrowRightTwoTone";

// Define the props for the SideMenu component
interface SideMenuProps {
  onToggle: (isCollapsed: boolean) => void;
}

// Main SideMenu component
const SideMenu: React.FC<SideMenuProps> = ({ onToggle }) => {
  const router = useRouter(); // Router hook for navigation
  const pathname = usePathname(); // Hook to get the current pathname

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

  // Function to toggle the collapse state of the menu
  const toggleMenu = () => {
    setIsCollapsed((prevIsCollapsed) => {
      const newIsCollapsed = !prevIsCollapsed;
      onToggle(newIsCollapsed);
      return newIsCollapsed;
    });
  };

  // Function to handle item click events
  const handleItemClick = (
    item: string,
    event: React.MouseEvent<HTMLElement>
  ) => {
    if (isDisabled(item)) {
      event.preventDefault(); // Prevent default behavior if the item is disabled
      return;
    }
    event.preventDefault();
    setSelectedItem(item); // Set the clicked item as selected
    router.push(item); // Navigate to the clicked item's path
  };

  // Function to check if an item is disabled
  const isDisabled = (targetPath: string) =>
    pathname === "/pages/projects" && targetPath !== "/pages/projects";

  // Function to get the CSS class for an item based on its state
  const getItemClass = (targetPath: string) => {
    const isSelected =
      selectedItem === targetPath ||
      (targetPath === "/pages/projects/[project_id]" &&
        pathname?.startsWith("/pages/projects/") &&
        pathname !== "/pages/projects");

    return [
      "flex items-center block py-2 px-4 rounded transition",
      isSelected ? "bg-base-300" : "hover:bg-base-300",
      isDisabled(targetPath)
        ? "pointer-events-none opacity-50 cursor-not-allowed"
        : "",
    ].join(" ");
  };

  return (
    <aside
      className={`fixed ${
        isCollapsed ? "w-20" : "w-64"
      } bg-secondary text-base-100 h-screen mt-16 p-4 transition-width duration-300`}
    >
      {/* Button to toggle menu collapse state */}
      <div className="flex justify-end">
        <button onClick={toggleMenu} className="p-2">
          {isCollapsed ? (
            <KeyboardArrowRightTwoToneIcon />
          ) : (
            <KeyboardArrowLeftTwoToneIcon />
          )}
        </button>
      </div>

      {/* Projects section */}
      <ul>
        <li>
          <Link
            href="/pages/projects"
            onClick={(e) => handleItemClick("/pages/projects", e)}
            className={getItemClass("/pages/projects")}
          >
            <OtherHousesOutlinedIcon />
            {!isCollapsed && <p className="ml-2">All Projects</p>}
          </Link>
        </li>
        <li>
          <button
            onClick={(e) => handleItemClick("/pages/projects/[project_id]", e)}
            className={getItemClass("/pages/projects/[project_id]")}
          >
            <ListAltOutlinedIcon />
            {!isCollapsed && <p className="ml-2">Current Project</p>}
          </button>
        </li>
      </ul>

      {/* Your Data section */}
      {!isCollapsed && <p className="text-sm mt-4">Your Data</p>}
      <ul>
        <li>
          <Link
            href="#data-viewer"
            onClick={(e) => handleItemClick("#data-viewer", e)}
            className={getItemClass("#data-viewer")}
          >
            <ManageSearchIcon />
            {!isCollapsed && <p className="ml-2">Data Viewer</p>}
          </Link>
        </li>
        <li>
          <Link
            href="#timeseries-viewer"
            onClick={(e) => handleItemClick("#timeseries-viewer", e)}
            className={getItemClass("#timeseries-viewer")}
          >
            <TimelineIcon />
            {!isCollapsed && <p className="ml-2">Timeseries Viewer</p>}
          </Link>
        </li>
        <li>
          <Link
            href="/pages/file_viewer"
            onClick={(e) => handleItemClick("/pages/file_viewer", e)}
            className={getItemClass("/pages/file_viewer")}
          >
            <FindInPageOutlinedIcon />
            {!isCollapsed && <p className="ml-2">File Viewer</p>}
          </Link>
        </li>
      </ul>

      {/* Data Management section */}
      {!isCollapsed && <p className="text-sm mt-4">Data Management</p>}
      <ul>
        <li>
          <Link
            href="#ontology"
            onClick={(e) => handleItemClick("#ontology", e)}
            className={getItemClass("#ontology")}
          >
            <CoronavirusOutlinedIcon />
            {!isCollapsed && <p className="ml-2">Ontology</p>}
          </Link>
        </li>
        <li>
          <Link
            href="/pages/data_source"
            onClick={(e) => handleItemClick("/pages/data_source", e)}
            className={getItemClass("/pages/data_source")}
          >
            <InboxIcon />
            {!isCollapsed && <p className="ml-2">Data Source</p>}
          </Link>
        </li>
        <li>
          <Link
            href="/pages/data_catalog"
            onClick={(e) => handleItemClick("/pages/data_catalog", e)}
            className={getItemClass("/pages/data_catalog")}
          >
            <InboxIcon />
            {!isCollapsed && <p className="ml-2">Data Catalog</p>}
          </Link>
        </li>
        <li>
          <Link
            href="#tags"
            onClick={(e) => handleItemClick("#tags", e)}
            className={getItemClass("#tags")}
          >
            <SellOutlinedIcon />
            {!isCollapsed && <p className="ml-2">Tags</p>}
          </Link>
        </li>
      </ul>

      {/* Widgets section */}
      {!isCollapsed && <p className="text-sm mt-4">Widgets</p>}
      <ul>
        <li>
          <Link
            href="#model-viewer"
            onClick={(e) => handleItemClick("#model-viewer", e)}
            className={getItemClass("#model-viewer")}
          >
            <ViewInArOutlinedIcon />
            {!isCollapsed && <p className="ml-2">Model Viewer</p>}
          </Link>
        </li>
        <li>
          <Link
            href="#events"
            onClick={(e) => handleItemClick("#events", e)}
            className={getItemClass("#events")}
          >
            <CalendarMonthOutlinedIcon />
            {!isCollapsed && <p className="ml-2">Events</p>}
          </Link>
        </li>
      </ul>
    </aside>
  );
};

export default SideMenu; // Export the SideMenu component as default
