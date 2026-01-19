import { useLanguage } from "@/app/contexts/Language";
import { OrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { PencilIcon, PlusIcon, TrashIcon } from "@heroicons/react/24/outline";
import { useState } from "react";
import { OrganizationResponseDto } from "../../types/responseDTOs";
import { Column } from "../../types/types";
import EditOrganization from "../SiteManagementPortal/EditOrganizationModal";
import CreateObjectStorage from "./CreateObjectStorage";

interface OrganizationManagementProps {
  organization: OrganizationSession;
}

const ObjectStorageTable = ({ organization }: OrganizationManagementProps) => {
  const { t } = useLanguage();
  const [isOrganizationModalOpen, setIsOrganizationModalOpen] = useState(false);
  const [editOrganizationModal, setEditOrganizationModal] = useState(false);
  const [selectedOrganizationId, setSelectedOrganizationId] = useState<
    number | null
  >(null);
  const [selectedOrganizationName, setSelectedOrganizationName] =
    useState<string>("");
  const [selectedOrganizationDescription, setSelectedOrganizationDescription] =
    useState<string>("");
  const [selectedOrganizations, setSelectedOrganizations] = useState<boolean[]>(
    []
  );
  const [selectAll, setSelectAll] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refreshOrganizations = async () => {
    try {
    } catch (err) {
      console.error("Failed to refresh object storage:", err);
      setError("Failed to refresh object storage.");
    }
  };

  const handleSelectAll = () => {};

  const handleCheckboxChange = (index: number) => {
    const next = [...selectedOrganizations];
    next[index] = !next[index];
    setSelectedOrganizations(next);
    setSelectAll(next.every(Boolean));
  };

  const handleDelete = async (index: number) => {
    try {
      // await archiveOrganization(organizationId);
      // setData((prev) => prev.filter((_, i) => i !== index));
    } catch (err) {
      console.error("Failed to delete object storage:", err);
      setError("Failed to delete object storage.");
    }
  };

  const handleDeleteSelected = async () => {
    // const selectedOrgIds = data
    //     .filter((_, i) => selectedOrganizations[i])
    //     .map((org) => org.id);
    // try {
    //     await Promise.all(selectedOrgIds.map((orgId) => archiveOrganization(orgId as number)));
    //     setData((prev) => prev.filter((_, i) => !selectedOrganizations[i]));
    // } catch (err) {
    //     console.error("Failed to delete selected organizations:", err);
    //     setError("Failed to delete selected organizations.");
    // }
  };

  const multipleSelected = () =>
    selectedOrganizations.filter(Boolean).length > 1;

  const openEditModal = (
    organizationId: number,
    organizationName: string,
    organizationDescription: string
  ) => {
    setSelectedOrganizationId(organizationId);
    setSelectedOrganizationName(organizationName);
    setSelectedOrganizationDescription(organizationDescription);
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
          <button
            onClick={() =>
              openEditModal(
                row.id as number,
                row.name,
                row.description as string
              )
            }
          >
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
      {/* <GenericTable
                columns={columns}
                data={data}
                enablePagination
            /> */}
      <CreateObjectStorage
        isOpen={isOrganizationModalOpen}
        onClose={() => setIsOrganizationModalOpen(false)}
        onOrganizationCreated={refreshOrganizations}
      />
      {selectedOrganizationId !== null && (
        <EditOrganization
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

export default ObjectStorageTable;
