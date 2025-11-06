"use client";

import React, { useState } from "react";
import Tabs from "../components/Tabs";
import { OauthApplicationResponseDto, OrganizationResponseDto } from "../types/responseDTOs";
import UsersTable from "../components/UsersTable";
import OAuthManagement from "../components/OAuthTable";
import OrganizationManagement from "../components/OrgTable";

interface SysAdminProps {
  organizations: OrganizationResponseDto[];
  applications: OauthApplicationResponseDto[];
}

const SysAdminClient = ({ organizations, applications }: SysAdminProps) => {
  const [activeTab, setActiveTab] = useState("Organization Management");

  const tabData = [
    {
      label: "Organization Management",
      content: <OrganizationManagement initialOrganizations={organizations} />,
    },
    {
      label: "Oauth Application",
      content: <OAuthManagement applications={applications} />,
    },
    {
      label: "Member Management",
      content: <UsersTable />,
    }
  ];

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  return (
    <div>
      <div className="bg-base-200/40 pl-12 p-4">
        <h1 className="text-2xl font-bold text-base-content">Site Management</h1>
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