"use client";

import React, { useState } from "react";
import Tabs from "../components/Tabs";
import {
  OauthApplicationResponseDto,
  OrganizationResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import UsersTable from "../components/users/UsersTable";
import OAuthManagement from "../components/SiteManagementPortal/OAuthTable";
import SiteOrganizationManagement from "../components/SiteManagementPortal/SiteOrgTable";
import { getAllOrganizations } from "@/app/lib/client_service/organization_services.client";
import { getAllOauthApplications } from "@/app/lib/client_service/oauth_services.client";
import { getAllUsers } from "@/app/lib/client_service/user_services.client";

interface SysAdminProps {
  organizations: OrganizationResponseDto[];
  applications: OauthApplicationResponseDto[];
  members: UserResponseDto[];
}

const SysAdminClient = ({
  organizations: initialOrganizations,
  applications: initialApplications,
  members: initialMembers,
}: SysAdminProps) => {
  const [activeTab, setActiveTab] = useState("");
  const [organizations, setOrganizations] = useState<OrganizationResponseDto[]>(initialOrganizations);
  const [applications, setApplications] = useState<OauthApplicationResponseDto[]>(initialApplications);
  const [members, setMembers] = useState<UserResponseDto[]>(initialMembers);


  const refreshOrganizations = async () => {
    try {
      const updatedData = await getAllOrganizations();
      setOrganizations(updatedData);
    } catch (err) {
      console.error("Failed to refresh organizations:", err);
    }
  };

  const refreshApplications = async () => {
    try {
      const updatedData = await getAllOauthApplications();
      setApplications(updatedData);
    } catch (err) {
      console.error("Failed to refresh organizations:", err);
    }
  };
  const refreshUsers = async () => {
    try {
      const updatedData = await getAllUsers();
      setMembers(updatedData);
    } catch (err) {
      console.error("Failed to refresh organizations:", err);
    }
  };

  const tabData = [
    {
      label: "Organization Management",
      content: (
        <SiteOrganizationManagement
          initialOrganizations={organizations}
          onOrganizationsChange={refreshOrganizations}
        />
      ),
    },
    {
      label: "Oauth Application",
      content: <OAuthManagement
        initialApplications={applications}
        onApplicationsChange={refreshApplications} />,
    },
    {
      label: "Member Management",
      content: <UsersTable
        initialMembers={members}
        header={"Site Users"}
        description={"Manage users for the site. Invite new users via email."}
        onUsersChange={refreshUsers} />,
    },
  ];

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  return (
    <div>
      <div className="bg-base-200/40 pl-12 p-6">
        <h1 className="text-2xl font-bold text-base-content">
          Site Management
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

export default SysAdminClient;