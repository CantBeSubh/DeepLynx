"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { getAllGroups } from "@/app/lib/client_service/group_services.client"; // Add this import
import { useState } from "react";
import Tabs from "../components/Tabs";
import UsersTable from "../components/users/UsersTable";
import {
  GroupResponseDto,
  PermissionResponseDto,
  ProjectResponseDto,
  RoleResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import InlineGroupsTable from "./groups/InlineGroupsTable";
import RolesAndPermissions from "./roles_and_permissions/RolesAndPermissions";
import OrganizationSettings from "./settings/OrganizationSettings";
import TagManagementClient from "./tag_management/TagManagementClient";

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
  const [groups, setGroups] = useState<GroupResponseDto[]>(initialGroups);
  const { t } = useLanguage();
  const { organization } = useOrganizationSession();

  const refreshGroups = async () => {
    if (!organization?.organizationId) return;

    try {
      const updatedGroups = await getAllGroups(
        organization.organizationId as number,
      );
      setGroups(updatedGroups);
    } catch (err) {
      console.error("Failed to refresh groups:", err);
    }
  };

  const tabData = [
    {
      label: t.translations.USERS,
      content: (
        <UsersTable
          initialMembers={members}
          header={"Organization Users"}
          description={t.translations.MANAGE_USERS_IN_YOUR_ORG_INVITE_VIA_EMAIL}
        />
      ),
    },
    {
      label: t.translations.ROLES_AND_PERMISSIONS,
      content: (
        <RolesAndPermissions
          initialRoles={initialRoles}
          initialPermissions={initialPermissions}
        />
      ),
    },
    {
      label: t.translations.GROUPS,
      content: (
        <InlineGroupsTable
          initialGroups={groups}
          availableUsers={members}
          organizationId={organization?.organizationId}
          onGroupsChange={refreshGroups}
        />
      ),
    },
    {
      label: t.translations.TAGS_AND_SECURITY_LABELS,
      content: <TagManagementClient projects={initialProjects} />,
    },
    {
      label: t.translations.SETTINGS,
      content: organization ? (
        <OrganizationSettings />
      ) : (
        <div>{t.translations.NO_ORG_SELECTED}</div>
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
