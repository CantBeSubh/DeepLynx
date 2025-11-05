"use client";

import { PlusIcon, TrashIcon, PencilIcon } from "@heroicons/react/24/outline";
import React, { useState, useEffect } from "react";
import Tabs from "../components/Tabs";
import { useLanguage } from "@/app/contexts/Language";
import GenericTable from "../components/GenericTable";
import { Column } from "../types/types";
import CreateOrganization from "../components/CreateOrganizationModal";
import { getAllOrganizations, archiveOrganization } from "@/app/lib/organization_services.client";
import { OauthApplicationResponseDto, OrganizationResponseDto } from "../types/responseDTOs";
import UsersTable from "../components/UsersTable";
import { LargeNumberLike } from "crypto";
import EditOrganizataion from "../components/EditOrganizationModal";
import OAuthManagement from "../components/OAuthTable";
// import EditOrganization from "../components/EditOrganizationModal";

interface SysAdminProps {
  organizations: OrganizationResponseDto[];
  applications: OauthApplicationResponseDto[];
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
      content: <OAuthManagement />,
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
  const [data, setData] = useState<OrganizationResponseDto[]>(initialOrganizations);
  const [isOrganizationModalOpen, setIsOrganizationModalOpen] = useState(false);
  const [editOrganizationModal, setEditOrganizationModal] = useState(false);
  const [selectedOrganizationId, setSelectedOrganizationId] = useState<number | null>(null);
  const [selectedOrganizationName, setSelectedOrganizationName] = useState<string>("");
  const [selectedOrganizationDescription, setSelectedOrganizationDescription] = useState<string>("");
  const [selectedOrganizations, setSelectedOrganizations] = useState<boolean[]>([]);
  const [selectAll, setSelectAll] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setSelectedOrganizations(new Array(data.length).fill(false));
    setSelectAll(false);
  }, [data.length]);

  const refreshOrganizations = async () => {
    try {
      const updatedData = await getAllOrganizations();
      setData(updatedData);
    } catch (err) {
      console.error("Failed to refresh organizations:", err);
      setError("Failed to refresh organizations.");
    }
  };

  const handleSelectAll = () => {
    const next = !selectAll;
    setSelectAll(next);
    setSelectedOrganizations(new Array(data.length).fill(next));
  };

  const handleCheckboxChange = (index: number) => {
    const next = [...selectedOrganizations];
    next[index] = !next[index];
    setSelectedOrganizations(next);
    setSelectAll(next.every(Boolean));
  };

  const handleDelete = async (index: number) => {
    const organizationId = data[index].id as number;
    try {
      await archiveOrganization(organizationId);
      setData((prev) => prev.filter((_, i) => i !== index));
    } catch (err) {
      console.error("Failed to delete organization:", err);
      setError("Failed to delete organization.");
    }
  };

  const handleDeleteSelected = async () => {
    const selectedOrgIds = data
      .filter((_, i) => selectedOrganizations[i])
      .map((org) => org.id);
    try {
      await Promise.all(selectedOrgIds.map((orgId) => archiveOrganization(orgId as number)));
      setData((prev) => prev.filter((_, i) => !selectedOrganizations[i]));
    } catch (err) {
      console.error("Failed to delete selected organizations:", err);
      setError("Failed to delete selected organizations.");
    }
  };

  const multipleSelected = () => selectedOrganizations.filter(Boolean).length > 1;

  const openEditModal = (organizationId: number, organizationName: string, organizationDescription: string) => {
    setSelectedOrganizationId(organizationId);
    setSelectedOrganizationName(organizationName);
    setSelectedOrganizationDescription(organizationDescription)
    setEditOrganizationModal(true);
  };

  const columns: Column<OrganizationResponseDto>[] = [
    {
      header: (
        <input
          type="checkbox"
          className="checkbox"
          checked={selectAll}
          onChange={handleSelectAll}
        />
      ),
      cell: (_row, index) => (
        <input
          type="checkbox"
          className="checkbox"
          checked={!!selectedOrganizations[index]}
          onChange={() => handleCheckboxChange(index)}
        />
      ),
      sortable: false,
    },
    {
      header: t.translations.NAME,
      data: "name" as keyof OrganizationResponseDto,
    },
    {
      header: t.translations.DESCRIPTION,
      data: "description" as keyof OrganizationResponseDto,
    },
    {
      header: "",
      cell: (row) => (
        <div className="flex">
          <button onClick={() => openEditModal(row.id as number, row.name, row.description as string)}>
            <PencilIcon className="size-6 text-secondary" />
          </button>
        </div>
      ),
      sortable: false,
    },
    {
      header: (
        <div className="flex">
          {multipleSelected() && (
            <button onClick={handleDeleteSelected}>
              <TrashIcon className="size-6 text-red-500" />
            </button>
          )}
        </div>
      ),
      cell: (_row, index) => (
        <div className="flex">
          <button onClick={() => handleDelete(index)}>
            <TrashIcon className="size-6 text-red-500" />
          </button>
        </div>
      ),
      sortable: false,
    },
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
      {error && <div className="p-4 text-red-500">{error}</div>}
      <GenericTable
        columns={columns}
        data={data}
        enablePagination
      />
      <CreateOrganization
        isOpen={isOrganizationModalOpen}
        onClose={() => setIsOrganizationModalOpen(false)}
        onOrganizationCreated={refreshOrganizations}
      />
      {selectedOrganizationId !== null && (
        <EditOrganizataion
          isOpen={editOrganizationModal}
          onClose={() => setEditOrganizationModal(false)}
          organizationId={selectedOrganizationId}
          organizationName={selectedOrganizationName}
          organizationDescription={selectedOrganizationDescription}
          onOrganizationUpdated={refreshOrganizations}
        />
      )}
    </div>
  );
};

export default SysAdminClient;