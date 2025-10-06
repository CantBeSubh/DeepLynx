"use client";

import React, { useState, useEffect } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import { projectMembers, defaultRoles } from "../../dummy_data/data";
import Tabs from "../Tabs";
import AddProjectMember from "@/app/(home)/components/ProjectSettingsTable/ProjectModals/ProjectMemberModal";
import MembersTable from '././ProjectTables/MembersTable';
import RolesTable from '././ProjectTables/RolesTable';
// import DataSourceTable from '././ProjectTables/DataSourceTable';
// import ObjectStorageTable from '././ProjectTables/ObjectStorageTable';
// import MemberSearchBar from './MemberSearchBar';
import { useRouter, useSearchParams } from "next/navigation";
import { PlusIcon } from "@heroicons/react/24/outline";

interface ProjectSettingsProps {
  className?: string;
}

const ProjectSettings = ({ className }: ProjectSettingsProps) => {
  const { t } = useLanguage();
  const [addProjectMemberModal, setAddProjectMemberModal] = useState(false);
  const [activeTab, setActiveTab] = useState("Members");
  const router = useRouter();
  const searchParams = useSearchParams();

  const tabData = [
    {
      label: "Members",
      content: (
        <MembersTable
          data={projectMembers}
        />
      ),
    },
    {
      label: "Roles",
      content: (
        <RolesTable
          data={defaultRoles}
        />
      ),
    },
    //  TODO POST FY: ADD BACK DATA SOURCE / OBJ STORAGE
    // {
    //   label: "Data Source",
    //   content: (
    //     <DataSourceTable
    //       data={mySavedSearches}
    //     />
    //   ),
    // },
    // {
    //   label: "Object Storage",
    //   content: (
    //     <ObjectStorageTable
    //       data={mySavedSearches}
    //     />
    //   ),
    // },
  ];

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  const handleAddButtonClick = (event: React.MouseEvent<HTMLElement>) => {
    event.preventDefault();
    if (activeTab === "Roles") {
      router.push("/project_settings/project_roles");
    } else if (activeTab === "Members") {
      setAddProjectMemberModal(true);
    }
  };

   // Effect to set the active tab from the query parameter
  useEffect(() => {
    const tab = searchParams.get('tab');
    if (tab) {
      setActiveTab(tab);
    }
  }, [searchParams]);

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <div className="flex justify-between items-start">
          <h2 className="card-title">{t.translations.PROJECT_SETTINGS}</h2>
          <div className="flex space-x-4">
            <button
              onClick={handleAddButtonClick}
              className="btn btn-secondary text-white"
            >
              <PlusIcon className="size-6" />
              {activeTab === "Members" ? t.translations.MEMBER : t.translations.ROLE}
            </button>
            <div className="flex flex-col">
              {/* TODO POST FY
              {activeTab === "Members" && <MemberSearchBar />} */}
            </div>
          </div>
        </div>
        <Tabs
          tabs={tabData}
          className="tabs tabs-border"
          onTabChange={handleTabChange}
          activeTab={activeTab}
        />
      </div>

      <AddProjectMember
        isOpen={addProjectMemberModal}
        onClose={() => setAddProjectMemberModal(false)}
      />
    </div>
  );
};

export default ProjectSettings;