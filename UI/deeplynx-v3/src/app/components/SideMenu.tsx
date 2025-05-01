import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
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
import Link from "next/link";

const SideMenu: React.FC = () => {
  const router = useRouter();
  const [selectedItem, setSelectedItem] = useState<string>("");

  useEffect(() => {
    if (typeof window !== "undefined") {
      const savedSelectedItem = localStorage.getItem("selectedItem");
      if (savedSelectedItem) {
        setSelectedItem(savedSelectedItem);
      }
    }
  }, []);

  useEffect(() => {
    if (typeof window !== "undefined") {
      localStorage.setItem("selectedItem", selectedItem);
    }
  }, [selectedItem]);

  const handleItemClick = (
    item: string,
    event: React.MouseEvent<HTMLAnchorElement>
  ) => {
    event.preventDefault();
    setSelectedItem(item);
    router.push(item);
  };

  return (
    <aside className="fixed w-64 bg-gray-800 text-white h-screen p-4 bg-secondary">
      <ul>
        <li>
          <Link
            href="/pages/projects"
            onClick={(event) => handleItemClick("/pages/projects", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "/pages/projects"
                ? "bg-gray-700"
                : "hover:bg-gray-700"
            }`}
          >
            <OtherHousesOutlinedIcon /> <p className="ml-2">All Projects</p>
          </Link>
        </li>
        <li>
          <a
            href="#"
            onClick={(event) =>
              handleItemClick("#current-project-dashboard", event)
            }
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "#current-project-dashboard"
                ? "bg-gray-700"
                : "hover:bg-gray-700"
            }`}
          >
            <ListAltOutlinedIcon />
            <p className="ml-2">Current Project Dashboard</p>
          </a>
        </li>
      </ul>
      <p className="text-sm mt-4">Your Data</p>
      <ul>
        <li>
          <a
            href="#"
            onClick={(event) => handleItemClick("#data-viewer", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "#data-viewer"
                ? "bg-gray-700"
                : "hover:bg-gray-700"
            }`}
          >
            <ManageSearchIcon /> <p className="ml-2">Data Viewer</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            onClick={(event) => handleItemClick("#timeseries-viewer", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "#timeseries-viewer"
                ? "bg-gray-700"
                : "hover:bg-gray-700"
            }`}
          >
            <TimelineIcon />
            <p className="ml-2">Timeseries Viewer</p>
          </a>
        </li>
        <li>
          <a
            href="/pages/file_viewer"
            onClick={(event) => handleItemClick("/pages/file_viewer", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "/pages/file_viewer"
                ? "bg-gray-700"
                : "hover:bg-gray-700"
            }`}
          >
            <FindInPageOutlinedIcon />
            <p className="ml-2">File Viewer</p>
          </a>
        </li>
      </ul>
      <p className="text-sm mt-4">Data Management</p>
      <ul>
        <li>
          <a
            href="#"
            onClick={(event) => handleItemClick("#ontology", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "#ontology" ? "bg-gray-700" : "hover:bg-gray-700"
            }`}
          >
            <CoronavirusOutlinedIcon /> <p className="ml-2">Ontology</p>
          </a>
        </li>
        <li>
          <a
            href="/pages/data_source"
            onClick={(event) => handleItemClick("/pages/data_source", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "/pages/data_source"
                ? "bg-gray-700"
                : "hover:bg-gray-700"
            }`}
          >
            <InboxIcon />
            <p className="ml-2">Data Source</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            onClick={(event) => handleItemClick("#tags", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "#tags" ? "bg-gray-700" : "hover:bg-gray-700"
            }`}
          >
            <SellOutlinedIcon />
            <p className="ml-2">Tags</p>
          </a>
        </li>
      </ul>
      <p className="text-sm mt-4">Widgets</p>
      <ul>
        <li>
          <a
            href="#"
            onClick={(event) => handleItemClick("#model-viewer", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "#model-viewer"
                ? "bg-gray-700"
                : "hover:bg-gray-700"
            }`}
          >
            <ViewInArOutlinedIcon /> <p className="ml-2">Model Viewer</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            onClick={(event) => handleItemClick("#events", event)}
            className={`flex items-center block py-2 px-4 rounded ${
              selectedItem === "#events" ? "bg-gray-700" : "hover:bg-gray-700"
            }`}
          >
            <CalendarMonthOutlinedIcon />
            <p className="ml-2">Events</p>
          </a>
        </li>
      </ul>
    </aside>
  );
};

export default SideMenu;
