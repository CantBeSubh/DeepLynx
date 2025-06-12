import React, { useState } from "react";

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
  className = "", // Default class name if not provided
}) => {
  const [activeIndex, setActiveIndex] = useState(0); // State to track the currently active tab index

  return (
    <div className={`${className}`}>
      {tabs.map((tab, index) => (
        <React.Fragment key={index}>
          <input
            type="radio"
            name={`tabs_${index}`}
            className="tab ml-2 border-neutral text-neutral hover:text-neutral"
            aria-label={tab.label} // Accessible label for the radio button
            checked={activeIndex === index} // Check if this tab is active
            onChange={() => setActiveIndex(index)} // Change active tab on selection
          />
          <div
            className={`tab-content p-6 ${
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
