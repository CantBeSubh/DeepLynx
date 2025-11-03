"use client";

import React, { useState, useEffect } from "react";
import GenericTable from "@/app/(home)/components/GenericTable";
import { Column } from "@/app/(home)/types/types";
import {
  getAllEventsPaginated,
  EventFilterParams,
} from "@/app/lib/event_services.client";
import { EventResponseDto, PaginatedEventsResponseDto } from "../types/responseDTOs";

const EventsHistoryClient = () => {
  const [data, setData] = useState<EventResponseDto[]>([]);
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 500,
    totalCount: 0,
  });
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<EventFilterParams>({});

  // Fetch events from backend
  const fetchEvents = async (pageNumber: number, pageSize: number) => {
    setLoading(true);
    try {
      const result: PaginatedEventsResponseDto = await getAllEventsPaginated({
        pageNumber,
        pageSize,
        ...filters,
      });

      setData(result.items);
      setPagination({
        pageNumber: result.pageNumber,
        pageSize: result.pageSize,
        totalCount: result.totalCount,
      });
    } catch (error) {
      console.error("Failed to fetch events:", error);
      setData([]);
      setPagination({
        pageNumber: 1,
        pageSize: 500,
        totalCount: 0,
      });
    } finally {
      setLoading(false);
    }
  };

  // Initial fetch
  useEffect(() => {
    fetchEvents(1, 500);
  }, []);

  // Handle page changes
  const handlePageChange = (pageNumber: number) => {
    fetchEvents(pageNumber, pagination.pageSize);
  };

  // Handle page size changes (optional)
  const handlePageSizeChange = (pageSize: number) => {
    fetchEvents(1, pageSize);
  };

  // Define columns
  const columns: Column<EventResponseDto>[] = [
    {
      header: "Timestamp",
      data: "lastUpdatedAt",
      sortable: false,
      // cell: (row) => {
      //     const date = new Date(row.lastUpdatedAt);
      //     return date.toLocaleString();
      // },
    },
    {
      header: "User",
      data: "lastUpdatedBy",
      sortable: false,
    },
    {
      header: "Project Name",
      data: "projectName",
      sortable: false,
    },
    {
      header: "Operation",
      data: "operation",
      sortable: false,
    },
    {
      header: "Entity Type",
      data: "entityType",
      sortable: false,
    },
    {
      header: "Entity Name",
      data: "entityName",
      sortable: false,
    },
    {
      header: "Data Source",
      data: "dataSourceName",
      sortable: false,
    },
    {
      header: "Updated By",
      data: "lastUpdatedBy",
      sortable: false,
    }
  ];

  return (
    <div className="px-8">
      <h1 className="text-2xl my-4 font-bold text-info-content">
        {/*{t.translations.DATA_CATALOG}*/}
        Event History
      </h1>
      {loading && (
        <div className="text-center py-4 text-base-content">
          Loading events...
        </div>
      )}
      <div className="flex">

        <div className="flex-1">
          <GenericTable
            columns={columns}
            gridView={true}
            data={data}
            enablePageLengthChange={true}
            enablePagination={true}
            backendPagination={true}
            paginationMetadata={pagination}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
            bordered={false}
            searchBar={true}
            rowsPerPage={500}
          />
        </div>
      </div>
    </div>
  );
};

export default EventsHistoryClient;
