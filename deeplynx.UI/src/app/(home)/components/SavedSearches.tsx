import React from "react";
import {
  myRecentSearches,
  peopleData,
  mySavedSearches,
} from "../dummy_data/data";
import GenericTable from "./GenericTable";
import { Column, MySearchsTable, PopularTable } from "../types/types";
import AvatarCell from "./Avatar";
import Tabs from "./Tabs";

interface SavedSearchProps {
  className?: string;
}

const SavedSearchesTabs = ({ className }: SavedSearchProps) => {
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
      label: "Recent",
      content: (
        <GenericTable
          columns={my_search_table_columns}
          data={myRecentSearches}
        />
      ),
    },
    {
      label: "Popular",
      content: (
        <GenericTable
          columns={popular_table_columns}
          data={peopleData}
          enablePagination
          rowsPerPage={5}
        />
      ),
    },
    {
      label: "My Searches",
      content: (
        <GenericTable
          columns={my_search_table_columns}
          data={mySavedSearches}
        />
      ),
    },
  ];

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md">
      <div className="card-body">
        <h2 className="card-title">Saved Searches</h2>
        <Tabs tabs={tabData} className="tabs tabs-border" />
      </div>
    </div>
  );
};

export default SavedSearchesTabs;
