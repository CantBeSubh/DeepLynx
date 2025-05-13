import React, { useState } from "react";
import AddCircleIcon from "@mui/icons-material/AddCircle";
import AttachFileIcon from "@mui/icons-material/AttachFile";
interface Tab {
  label: string;
  content: React.ReactNode;
}

interface TabsProps {
  tabs: Tab[];
  className?: string;
  showButtons?: boolean; // Add new prop
}

const Tabs: React.FC<TabsProps> = ({
  tabs,
  className = "tabs tabs-lift",
  showButtons = false,
}) => {
  const [activeIndex, setActiveIndex] = useState(0);

  return (
    <div className={`${className} relative`}>
      {showButtons && (
        <div className="absolute top-0 right-0 flex space-x-4 mt-2 mr-2">
          <button>
            <AttachFileIcon className="text-primary" />
          </button>
        </div>
      )}
      {tabs.map((tab, index) => (
        <React.Fragment key={index}>
          <input
            type="radio"
            name="tabs"
            className="tab"
            aria-label={tab.label}
            checked={activeIndex === index}
            onChange={() => setActiveIndex(index)}
          />
          <div
            className={`tab-content bg-base-100 border-base-300 p-6 ${
              activeIndex === index ? "" : "hidden"
            }`}
          >
            {tab.content}
          </div>
        </React.Fragment>
      ))}
    </div>
  );
};

export default Tabs;
