"use client";

import React, { useState } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import Link from "next/link";
import { myRecentSearches, mySavedSearches } from "../../dummy_data/data";
import { Column, MySearchsTable, PopularTable } from "../../types/types";
import AvatarCell from "../Avatar";
import GenericTable from "../GenericTable";
import Tabs from "../Tabs";
import { PlusIcon, TrashIcon } from "@heroicons/react/24/outline";
import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import AddMember from "@/app/(home)/components/WidgetCards/WidgetCardModals/AddMemberModal";
import MembersTable from '././ProjectTables/MembersTable';
import RolesTable from '././ProjectTables/RolesTable';
import DataSourceTable from '././ProjectTables/DataSourceTable';
import ObjectStorageTable from '././ProjectTables/ObjectStorageTable';


interface ProjectSettingsProps {
  className?: string;
}

const ProjectSettings = ({ className }: ProjectSettingsProps) => {
  const { t } = useLanguage();
  const [addMemberModal, setAddMemberModal] = useState(false);
  const my_search_table_columns: Column<MySearchsTable>[] = [
    {
      header: "Name",
      data: "name",
    },
    {
      header: "Email",
      sortable: false,
    },
    {
      header: "",
      cell: () => (
        <div className="flex justify-end">
          <button className="btn">
            {" "}
            {t.translations.ROLE}
          </button>
        </div>
      ),
      sortable: false,
    },
    {
      header: "",
      cell: () => (
        <div className="flex justify-end">
          <TrashIcon className="size-6 red-icon"/>
        </div>
      ),
      sortable: false,
    }
  ];

  const popular_table_columns: Column<PopularTable>[] = [
    {
      header: "Created by",
      cell: (row) => (
        <div className="flex gap-4 items-center">
          <AvatarCell name={row.name} image={row.image} />
          {row.name}
        </div>
      ),
    },
    {
      header: "Search Nickname",
      data: "nickname",
    },
    {
      header: "Visibility",
      data: "visibility",
    },
  ];

  const tabData = [
    {
      label: "Members",
      content: (
        <MembersTable
          data={myRecentSearches}
        />
      ),
    },
    {
      label: "Roles",
      content: (
        <RolesTable
          data={mySavedSearches}
        />
      ),
    },
    {
      label: "Data Source",
      content: (
        <DataSourceTable
          data={mySavedSearches}
        />
      ),
    },
    {
      label: "Object Storage",
      content: (
        <ObjectStorageTable
          data={mySavedSearches}
        />
      ),
    },
  ];

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <div className="flex justify-between items-start">
            <h2 className="card-title">{t.translations.PROJECT_SETTINGS}</h2>
            <div className="flex space-x-4">
                <button
                    onClick={() => setAddMemberModal(true)}
                    className="btn btn-secondary text-white"
                >
                    <PlusIcon className="size-6" />
                    {t.translations.MEMBER}
                </button>
                <div className="flex flex-col">
                    <LargeSearchBar />
                </div>
            </div>
        </div>
        <Tabs tabs={tabData} className="tabs tabs-border" />
      </div>

      <AddMember
        isOpen={addMemberModal}
        onClose={() => setAddMemberModal(false)}
      />
    </div>
  );
};

export default ProjectSettings;
