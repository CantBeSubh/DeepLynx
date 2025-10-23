"use client";

import React, { useState } from "react";
import ProjectDropdown from "../components/ProjectDropdown";
import { useLanguage } from "@/app/contexts/Language";

interface Props {
  initialProjects: { id: string; name: string }[];
  initialSelectedProjects: string[];
}

const TagManagementClient = ({
  initialProjects,
  initialSelectedProjects,
}: Props) => {
  const { t } = useLanguage();
  const [projects] = useState(initialProjects);
  const [selectedProjects, setSelectedProjects] = useState<string[]>(
    initialSelectedProjects
  );
  const [selectedMenuItem, setSelectedMenuItems] = useState("Search Tags");

  const menuItems = ["Search Tags", "Create Tag", "Attach Tags", "Edit Tags"];
  return (
    <div>
      {/* Header */}
      <div className="items-center bg-base-200/40 pl-12 py-2 pb-4">
        <h1 className="text-2xl font-bold text-info-content">Tag Management</h1>
        <ProjectDropdown
          projects={projects}
          onSelectionChange={setSelectedProjects}
          defaultSelected={
            initialSelectedProjects.length ? initialSelectedProjects : undefined
          }
        />
      </div>

      {/* Content */}
      <div className="grid grid-cols-[20%_40%_40%] p-6">
        <div className="card shadow-xl rounded-lg p-6 mr-6">
          <ul>
            {menuItems.map((item) => (
              <li
                key={item}
                onClick={() => setSelectedMenuItems(item)}
                className={`cursor-pointer px-4 py-2 rounded-lg transition-colors font-bold ${
                  selectedMenuItem === item
                    ? "bg-info/50 text-info-content"
                    : "hover:bg-base-200"
                }`}
              >
                {item}
              </li>
            ))}
          </ul>
        </div>
        <div className="card shadow-xl rounded-lg p-6 mr-6">Tags</div>
        <div className="card shadow-xl rounded-lg p-6">Records</div>
      </div>
    </div>
  );
};

export default TagManagementClient;
