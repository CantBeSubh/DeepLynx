"use client";

import React, { useState } from "react";
import Tabs from "../components/Tabs";
import { OauthApplicationResponseDto, OrganizationResponseDto, UserResponseDto } from "../types/responseDTOs";
import UsersTable from "../components/SiteManagementPortal/UsersTable";
import OAuthManagement from "../components/SiteManagementPortal/OAuthTable";
import SiteOrganizationManagement from "../components/SiteManagementPortal/OrgTable";
import TagManagementPage from "../tag_management/page";
import OrganizationSettings from "../components/OrganizationManagementPortal/OrganizationSettings";
import { useLanguage } from "@/app/contexts/Language";
interface SysAdminProps {
  organizations: OrganizationResponseDto[];
  applications: OauthApplicationResponseDto[];
  members: UserResponseDto[];
}

const OrganizationManagementClient = ({ organizations, applications, members }: SysAdminProps) => {
  const [activeTab, setActiveTab] = useState("");
  const { t } = useLanguage();


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
      content: "content here",
    },
    {
      label: "Settings",
      content: <OrganizationSettings />,
    }
  ];

  const handleTabChange = (label: string) => {
    console.log(label)
    setActiveTab(label);
  };

  return (
    <div>
      <div className="bg-base-200/40 pl-12 p-4">
        <h1 className="text-2xl font-bold text-base-content">{t.translations.ORGANIZATION_MANAGEMENT}</h1>
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