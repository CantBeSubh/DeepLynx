"use client";

import React, { useState } from "react";
import Tabs from "../components/Tabs";
import {
  OauthApplicationResponseDto,
  OrganizationResponseDto,
  UserResponseDto,
} from "../types/responseDTOs";
import UsersTable from "../organization_management/users/UsersTable";
import OAuthManagement from "../components/SiteManagementPortal/OAuthTable";
import SiteOrganizationManagement from "../components/SiteManagementPortal/OrgTable";

interface SysAdminProps {
  organizations: OrganizationResponseDto[];
  applications: OauthApplicationResponseDto[];
  members: UserResponseDto[];
}

const SysAdminClient = ({
  organizations,
  applications,
  members,
}: SysAdminProps) => {
  const [activeTab, setActiveTab] = useState("");

  const tabData = [
    {
      label: "Organization Management",
      content: (
        <SiteOrganizationManagement initialOrganizations={organizations} />
      ),
    },
    {
      label: "Oauth Application",
      content: <OAuthManagement applications={applications} />,
    },
    {
      label: "Member Management",
      content: <UsersTable members={members} />,
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
