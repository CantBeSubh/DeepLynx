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
  return (
    <div>
      <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2 pb-4">
        <div>
          <h1 className="text-2xl font-bold text-info-content">
            Tag Management
          </h1>
          <ProjectDropdown
            projects={projects}
            onSelectionChange={setSelectedProjects}
            defaultSelected={
              initialSelectedProjects.length
                ? initialSelectedProjects
                : undefined
            }
          />
        </div>
      </div>
    </div>
  );
};

export default TagManagementClient;
