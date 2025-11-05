"use client";

import { PlusIcon } from "@heroicons/react/24/outline";
import React, { useState } from "react";
import Tabs from "../components/Tabs";
import { useLanguage } from "@/app/contexts/Language";
import { ExpandableTable } from "../components/ExpandableTable";
import GenericTable from "../components/GenericTable";
import { Column } from "../types/types";
import CreateOrganization from "../components/CreateOrganizationModal";
import { getAllOrganizations } from "@/app/lib/organization_services.client";
import { OrganizationResponseDto } from "../types/responseDTOs";
import UsersTable from "../components/UsersTable";

interface SysAdminProps {
  organizations: OrganizationResponseDto[];
}

const SysAdminClient = ({ organizations }: SysAdminProps) => {
  const [activeTab, setActiveTab] = useState("Organization Management");

  const tabData = [
    {
      label: "Organization Management",
      content: <OrganizationManagement initialOrganizations={organizations} />,
    },
    {
      label: "Oauth Application",
      content: <div>Once J2 finishes stuff</div>,
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

interface OrganizationManagementProps {
  initialOrganizations: OrganizationResponseDto[];
}

const OrganizationManagement = ({ initialOrganizations }: OrganizationManagementProps) => {
  const { t } = useLanguage();
  const [organizations, setOrganizations] =
    useState<OrganizationResponseDto[]>(initialOrganizations);
  const [isOrganizationModalOpen, setIsOrganizationModalOpen] = useState(false);
  const refreshOrganizations = async () => {
    try {
      const data = await getAllOrganizations();
      setOrganizations(data);
    } catch (err) {
      console.error("Failed to refresh projects:", err);
    }
  };

  const columns: Column<OrganizationResponseDto>[] = [
    {
      header: t.translations.NAME,
      data: "name" as keyof OrganizationResponseDto,
    },
    {
      header: t.translations.DESCRIPTION,
      data: "description" as keyof OrganizationResponseDto,
    }
  ];

  return (
    <div>
      <div className="flex justify-end p-4 mr-4">
        <button
          className="btn btn-secondary btn-sm flex-1 sm:flex-initial"
          data-tour="create-project"
          onClick={() => setIsOrganizationModalOpen(true)}
        >
          <PlusIcon className="size-5" />
          <span>{t.translations.ORGANIZATION}</span>
        </button>
      </div>
      <GenericTable
        columns={columns}
        data={organizations}
        enablePagination
      />
      <CreateOrganization
        isOpen={isOrganizationModalOpen}
        onClose={() => setIsOrganizationModalOpen(false)}
        onOrganizationCreated={refreshOrganizations}
      />
    </div>

  );
};

export default SysAdminClient;