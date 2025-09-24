"use client";

import React, { useState } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import { mySavedSearches, projectMembers } from "../../dummy_data/data";
import Tabs from "../Tabs";
import AddProjectMember from "@/app/(home)/components/ProjectSettingsTable/ProjectModals/ProjectMemberModal";
import MembersTable from '././ProjectTables/MembersTable';
import RolesTable from '././ProjectTables/RolesTable';
import DataSourceTable from '././ProjectTables/DataSourceTable';
import ObjectStorageTable from '././ProjectTables/ObjectStorageTable';
import MemberSearchBar from './MemberSearchBar';
import { PlusIcon } from "@heroicons/react/24/outline";

interface ProjectSettingsProps {
  className?: string;
}

const ProjectSettings = ({ className }: ProjectSettingsProps) => {
  const { t } = useLanguage();
  const [addProjectMemberModal, setAddProjectMemberModal] = useState(false);
  const [activeTab, setActiveTab] = useState("Members");

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
          data={mySavedSearches}
        />
      ),
    },
    {
      label: "Data Source",
      content: (
        <DataSourceTable
          data={mySavedSearches}
        />
      ),
    },
    {
      label: "Object Storage",
      content: (
        <ObjectStorageTable
          data={mySavedSearches}
        />
      ),
    },
  ];

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <div className="flex justify-between items-start">
          <h2 className="card-title">{t.translations.PROJECT_SETTINGS}</h2>
          <div className="flex space-x-4">
            <button
              onClick={() => setAddProjectMemberModal(true)}
              className="btn btn-secondary text-white"
            >
              <PlusIcon className="size-6" />
              {activeTab === "Members" ? t.translations.MEMBER : t.translations.ROLE}
            </button>
            <div className="flex flex-col">
              <MemberSearchBar />
            </div>
          </div>
        </div>
        <Tabs
          tabs={tabData}
          className="tabs tabs-border"
          onTabChange={handleTabChange}
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
