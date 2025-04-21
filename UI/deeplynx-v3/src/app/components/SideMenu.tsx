import React from "react";
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

const SideMenu = () => {
  return (
    <aside className="fixed w-64 bg-gray-800 text-white h-screen p-4 bg-secondary">
      <ul>
        <li>
          <a
            href="/pages/projects"
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
          >
            <OtherHousesOutlinedIcon /> <p className="ml-2">All Projects</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
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
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
          >
            <ManageSearchIcon /> <p className="ml-2">Data Viewer</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
          >
            <TimelineIcon />
            <p className="ml-2">Timeseries Viewer</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
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
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
          >
            <CoronavirusOutlinedIcon /> <p className="ml-2">Ontology</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
          >
            <InboxIcon />
            <p className="ml-2">Data Source</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
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
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
          >
            <ViewInArOutlinedIcon /> <p className="ml-2">Model Viewer</p>
          </a>
        </li>
        <li>
          <a
            href="#"
            className="flex items-center block py-2 px-4 hover:bg-gray-700 rounded"
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
