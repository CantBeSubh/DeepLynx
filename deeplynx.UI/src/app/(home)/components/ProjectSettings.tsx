"use client";

import { useLanguage } from "@/app/contexts/Language";
import Link from "next/link";
import { myRecentSearches, mySavedSearches } from "../dummy_data/data";
import { Column, MySearchsTable, PopularTable } from "../types/types";
import AvatarCell from "./Avatar";
import GenericTable from "./GenericTable";
import Tabs from "./Tabs";

interface ProjectSettingsProps {
  className?: string;
}

const ProjectSettings = ({ className }: ProjectSettingsProps) => {
  const { t } = useLanguage();
  const my_search_table_columns: Column<MySearchsTable>[] = [
    {
      header: "Name",
      data: "name",
    },
    {
      header: "Filters",
      cell: (row) =>
        row.filters.map((filter, index) => (
          <span key={index} className="badge badge-info mr-2 mt-2">
            {filter}
          </span>
        )),
      sortable: false,
    },
    {
      header: "Saved",
      data: "createdAt",
      sortable: false,
    },
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
      label: "General",
      content: (
        <GenericTable
          columns={my_search_table_columns}
          data={myRecentSearches}
          enablePagination
          rowsPerPage={5}
        />
      ),
    },
    {
      label: "Users",
      content: (
        <GenericTable
          columns={my_search_table_columns}
          data={mySavedSearches}
          enablePagination
          rowsPerPage={5}
        />
      ),
    },
    {
      label: "Data Source",
      content: (
        <GenericTable
          columns={my_search_table_columns}
          data={mySavedSearches}
          enablePagination
          rowsPerPage={5}
        />
      ),
    },
    {
      label: "Object Storage",
      content: (
        <GenericTable
          columns={my_search_table_columns}
          data={mySavedSearches}
          enablePagination
          rowsPerPage={5}
        />
      ),
    },
  ];

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <span className="flex justify-between">
          <h2 className="card-title">Project Settings</h2>
          <Link
            className="btn btn-secondary text-white"
            href="/savedsearchesplaceholder"
          >
            {t.translations.VISIT}
          </Link>
        </span>
        <Tabs tabs={tabData} className="tabs tabs-border" />
      </div>
    </div>
  );
};

export default ProjectSettings;
