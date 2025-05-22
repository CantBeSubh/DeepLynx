import React, { useState } from "react";
import AttachFileIcon from "@mui/icons-material/AttachFile";

// Define the structure of a Tab
interface Tab {
  label: string;
  content: React.ReactNode;
}

// Define the props for the Tabs component
interface TabsProps {
  tabs: Tab[];
  className?: string;
  showButtons?: boolean;
}

// Main Tabs component
const Tabs: React.FC<TabsProps> = ({
  tabs,
  className = "tabs tabs-lift", // Default class name if not provided
  showButtons = false, // Default value for showButtons
}) => {
  const [activeIndex, setActiveIndex] = useState(0); // State to track the currently active tab index

  return (
    <div className={`${className} relative`}>
      {/* Render buttons if showButtons is true */}
      {showButtons && (
        <div className="absolute top-0 right-0 flex space-x-4 mt-2 mr-2">
          <button>
            <AttachFileIcon className="text-primary" />
          </button>
        </div>
      )}
      {/* Render each tab */}
      {tabs.map((tab, index) => (
        <React.Fragment key={index}>
          {/* Radio input to control active tab */}
          <input
            type="radio"
            name="tabs"
            className="tab"
            aria-label={tab.label} // Accessible label for the radio button
            checked={activeIndex === index} // Check if this tab is active
            onChange={() => setActiveIndex(index)} // Change active tab on selection
          />
          {/* Content for the current tab */}
          <div
            className={`tab-content bg-base-100 border-base-300 p-6 ${
              activeIndex === index ? "" : "hidden" // Show content if tab is active
            }`}
          >
            {tab.content} {/* Render the content of the tab */}
          </div>
        </React.Fragment>
      ))}
    </div>
  );
};

export default Tabs;
