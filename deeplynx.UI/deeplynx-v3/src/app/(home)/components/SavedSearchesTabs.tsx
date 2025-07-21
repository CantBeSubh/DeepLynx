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

const SavedSearchesTabs = () => {
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
        <AvatarCell name={row.name} image={row.image} showName={true} />
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
        <Tabs tabs={tabData} className="tabs tabs-border" />
  );
};

export default SavedSearchesTabs;
