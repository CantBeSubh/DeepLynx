"use client";

import React, { useState, useEffect } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { systemUsers, systemGroups, systemOrgs } from "../../dummy_data/data";
import Tabs from "../Tabs";
import { useRouter, useSearchParams } from "next/navigation";
import MemberSearchBar from "../ProjectSettingsTable/MemberSearchBar";
import { PlusIcon } from "@heroicons/react/24/outline";
import UsersTable from "./MemberTables/UsersTable";
import GroupsTable from "./MemberTables/GroupsTable";
import OrganizationsTable from "./MemberTables/OrganizationsTable";

interface ProjectSettingsProps {
  className?: string;
}

const MemberSettings = ({ className }: ProjectSettingsProps) => {
  const { t } = useLanguage();
  const [addProjectMemberModal, setAddProjectMemberModal] = useState(false);
  const [activeTab, setActiveTab] = useState("Members");
  const router = useRouter();
  const searchParams = useSearchParams();

  const tabData = [
    {
      label: "Users",
      content: <UsersTable />,
    },
    {
      label: "Groups",
      content: <GroupsTable data={systemGroups} />,
    },
    {
      label: "Organizations",
      content: <OrganizationsTable data={systemOrgs} />,
    },
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
    const tab = searchParams.get("tab");
    if (tab) {
      setActiveTab(tab);
    }
  }, [searchParams]);

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <div className="flex justify-between items-start">
          <h2 className="card-title">{t.translations.MEMBER_MANAGEMENT}</h2>
          <div className="flex space-x-4">
            <button
              onClick={handleAddButtonClick}
              className="btn btn-secondary text-white"
            >
              <PlusIcon className="size-6" />
              {activeTab === "Users"
                ? t.translations.USER
                : t.translations.MEMBER}
            </button>
            <div className="flex flex-col">
              {activeTab === "Users" && <MemberSearchBar />}
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

      {/* TO DO: SWITCH TO ADD USER TO NEXUS
      <AddProjectMember
        isOpen={addProjectMemberModal}
        onClose={() => setAddProjectMemberModal(false)}
      /> */}
    </div>
  );
};

export default MemberSettings;
