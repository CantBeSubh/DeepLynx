"use client";

import React, { useState } from "react";

interface Tab {
  label: string;
  content: React.ReactNode;
}

interface TabsProps {
  tabs: Tab[];
  className?: string;
  onTabChange?: (label: string) => void;
}

const Tabs: React.FC<TabsProps> = ({ tabs, className = "", onTabChange }) => {
  const [activeIndex, setActiveIndex] = useState(0);

  const handleTabClick = (index: number, label: string) => {
    setActiveIndex(index);
    if (onTabChange) {
      onTabChange(label);
    }
  };

return (
    <div className={className}>
      {/* Tabs header */}
      <div className="tabs tabs-border border-b border-base-200">
        {tabs.map((tab, index) => (
          <a
            key={index}
            className={`tab tab-bordered ${
              activeIndex === index ? "tab-active text-secondary " : ""
            }`}
            onClick={() => handleTabClick(index, tab.label)}
          >
            {tab.label}
          </a>
        ))}
      </div>

      {/* Tab content */}
      <div className="flex justify-center items-start w-full">
        <div className="w-full">{tabs[activeIndex].content}</div>
      </div>
    </div>
  );
};

export default Tabs;
