// src/app/(home)/organization_management/OrganizationManagementClient.tsx
"use client";

import React, { useState } from "react";
import Tabs from "../components/Tabs";
import {
  GroupResponseDto,
  ProjectResponseDto,
  UserResponseDto,
  RoleResponseDto,
  PermissionResponseDto,
} from "../types/responseDTOs";
import UsersTable from "./users/UsersTable";
import { useLanguage } from "@/app/contexts/Language";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import InlineGroupsTable from "./groups/InlineGroupsTable";
import RolesAndPermissions from "./roles_and_permissions/RolesAndPermissions";
import OrganizationSettings from "./settings/OrganizationSettings";
import TagManagementClient from "./tag_management/TagManagementClient";
import OptionThree from "./tag_management/OptionThree";
import SettingsOne from "./settings/SettingsOne";
import SettingsTwo from "./settings/SettingsTwo";
import SettingsThree from "./settings/SettingsThree";

interface OrganizationManagementProps {
  members: UserResponseDto[];
  initialProjects: ProjectResponseDto[];
  initialGroups: GroupResponseDto[];
  initialRoles: RoleResponseDto[];
  initialSelectedProject?: ProjectResponseDto;
  initialPermissions: PermissionResponseDto[];
}

const OrganizationManagementClient = ({
  members,
  initialGroups,
  initialRoles,
  initialPermissions,
  initialProjects,
}: OrganizationManagementProps) => {
  const [activeTab, setActiveTab] = useState("");
  const { t } = useLanguage();
  const { organization } = useOrganizationSession();

  const tabData = [
    {
      label: "Users",
      content: <UsersTable members={members} />,
    },
    {
      label: "Roles & Permissions",
      content: (
        <RolesAndPermissions
          initialRoles={initialRoles}
          initialPermissions={initialPermissions}
        />
      ),
    },
    {
      label: "Groups",
      content: (
        <InlineGroupsTable
          initialGroups={initialGroups}
          availableUsers={members}
          organizationId={organization?.organizationId}
        />
      ),
    },
    {
      label: "Tags and Security Labels",
      // content: <TagManagementClient />,
      // content: <OptionOne />,
      // content: <OptionTwo />,
      content: <OptionThree projects={initialProjects} />,
    },
    {
      label: "Settings",
      content: organization ? (
        <OrganizationSettings organization={organization} />
      ) : (
        // <SettingsOne />
        // <SettingsTwo />
        // <SettingsThree />
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
