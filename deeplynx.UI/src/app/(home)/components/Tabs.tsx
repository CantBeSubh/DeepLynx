"use client";

import React, { useState } from "react";

interface Tab {
  label: string;
  content: React.ReactNode;
}

interface TabsProps {
  tabs: Tab[];
  className?: string;
}

const Tabs: React.FC<TabsProps> = ({ tabs, className = "" }) => {
  const [activeIndex, setActiveIndex] = useState(0);

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
            onClick={() => setActiveIndex(index)}
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
