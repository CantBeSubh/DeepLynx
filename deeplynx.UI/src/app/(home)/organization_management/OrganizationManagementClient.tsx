"use client";

import React, { useState } from "react";
import Tabs from "../components/Tabs";
import {
  OrganizationResponseDto,
  ProjectResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import UsersTable from "../components/SiteManagementPortal/UsersTable";
import OrganizationSettings from "../components/OrganizationManagementPortal/OrganizationSettings";
import { useLanguage } from "@/app/contexts/Language";
import ObjectStorageTable from "../components/OrganizationManagementPortal/ObjectStorageTable";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import TagManagementClient from "../tag_management/TagManagementClient";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";

interface OrganizationManagementProps {
  members: UserResponseDto[];
  initialProjects: ProjectResponseDto[];
  initialSelectedProjects?: ProjectResponseDto | null;
}

const OrganizationManagementClient = ({
  members,
  initialProjects,
  initialSelectedProjects,
}: OrganizationManagementProps) => {
  const [activeTab, setActiveTab] = useState("");
  const { t } = useLanguage();
  const { organization, setOrganization } = useOrganizationSession();
  const { project } = useProjectSession();
  console.log("Selected Project", project);

  const tabData = [
    {
      label: "Users",
      content: <UsersTable members={members} />,
    },
    {
      label: "Roles & Permissions",
      content: "content here",
    },
    {
      label: "Groups",
      content: "content here",
    },
    {
      label: "Tags and Security Labels",
      content: (
        <TagManagementClient
          initialProjects={initialProjects}
          initialSelectedProjects={initialSelectedProjects}
        />
      ),
    },
    // {
    //   label: "Object Storage",
    //   content: <ObjectStorageTable organization={organization} />
    // }, //TODO: Object storage org level management backend not finished yet
    {
      label: "Settings",
      content: organization ? (
        <OrganizationSettings organization={organization} />
      ) : (
        <div>No organization selected</div>
      ),
    },
  ];

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  return (
    <div>
      <div className="bg-base-200/40 pl-12 p-6">
        <h1 className="text-2xl font-bold text-base-content">
          {t.translations.ORGANIZATION_MANAGEMENT}
        </h1>
      </div>
      <div className="p-2">
        <Tabs
          tabs={tabData}
          className="tabs tabs-border ml-5"
          onTabChange={handleTabChange}
          activeTab={activeTab}
        />
      </div>
    </div>
  );
};

export default OrganizationManagementClient;
