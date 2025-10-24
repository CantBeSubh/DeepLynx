"use client";

import ServerPaginatedTable, { PaginationInfo } from "@/app/(home)/components/ServerPaginatedTable";
import { Column } from "@/app/(home)/types/types";
import { useEffect, useMemo, useState } from "react";
import { getAllEvents } from "@/app/lib/event_services.client";
import EventFilters from "@/app/(home)/components/EventFilters";

// Define the Event type based on the C# model
interface Event {
  id: number;
  operation: string;
  entityType: string;
  entityId: number | null;
  entityName: string | null;
  projectId: number | null;
  projectName: string | null;
  organizationId: number | null;
  dataSourceId: number | null;
  dataSourceName: string | null;
  properties: string;
  lastUpdatedBy: string | null;
  lastUpdatedAt: Date;
  summary?: React.ReactNode;
  dataSource?: string;
}

const EventHistory = () => {
  const [events, setEvents] = useState<Event[]>([]);
  const [paginationInfo, setPaginationInfo] = useState<PaginationInfo | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(false);

  // Fetch events with pagination
  const fetchEvents = async (page: number, size: number) => {
    setLoading(true);
    try {
      // Update getAllEvents to accept page and size parameters
      const fetchedEventData = await getAllEvents(page, size);
      
      setEvents(fetchedEventData.items);
      
      // Set pagination info from the API response
      setPaginationInfo({
        pageNumber: fetchedEventData.pageNumber,
        pageSize: fetchedEventData.pageSize,
        totalCount: fetchedEventData.totalCount,
        totalPages: fetchedEventData.totalPages,
        hasPrevious: fetchedEventData.hasPrevious,
        hasNext: fetchedEventData.hasNext,
      });
      
      console.log(fetchedEventData.items);
    } catch (e) {
      console.error('Failed to fetch events:', e);
    } finally {
      setLoading(false);
    }
  };

  // Fetch events when page or page size changes
  useEffect(() => {
    fetchEvents(currentPage, pageSize);
  }, [currentPage, pageSize]);

  // Handle page change
  const handlePageChange = (newPage: number) => {
    setCurrentPage(newPage);
  };

  // Handle page size change
  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setCurrentPage(1); // Reset to first page when changing page size
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const options: Intl.DateTimeFormatOptions = {
      month: "long",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "numeric",
      hour12: true,
      timeZoneName: "short",
    };
    return date.toLocaleString("en-US", options);
  };

  const formatEntityType = (type: string) => {
    const spaced = type.replace(/_/g, " ");
    const capitalized = spaced
      .split(" ")
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(" ");
    
    return capitalized;
  };

  // Define columns for the table
  const columns: Column<Event>[] = useMemo(() => [
    {
      header: "Event Occurred",
      data: "lastUpdatedAt",
      sortable: true,
      cell: (row) => formatDate(row.lastUpdatedAt.toString()),
    },
    {
      header: "Last Updated By",
      data: "lastUpdatedBy",
      sortable: true,
      cell: (row) => row.lastUpdatedBy ?? "Unknown",
    },
    {
      header: "Project Name",
      data: "projectName",
      sortable: true,
      cell: (row) => row.projectName ?? "N/A",
    },
    {
      header: "Operation",
      data: "operation",
      sortable: true,
      cell: (row) => (
        <span
          className={`badge ${
            row.operation === "CREATE"
              ? "badge-success"
              : row.operation === "UPDATE"
              ? "badge-warning"
              : "badge-error text-black"
          }`}
        >
          {row.operation}
        </span>
      ),
    },
    {
      header: "Entity Type",
      data: "entityType",
      sortable: true,
      cell: (row) => row.entityType ? formatEntityType(row.entityType) : "N/A",
    },
    {
      header: "Entity Name",
      data: "entityName",
      sortable: true,
      cell: (row) => row.entityName ?? "N/A",
    },
    {
      header: "Data Source",
      data: "dataSourceName",
      sortable: true,
      cell: (row) => row.dataSourceName ?? "N/A",
    }
  ], []);

  // Show loading state while fetching
  if (loading && !paginationInfo) {
    return (
      <div className="flex items-center justify-center p-4">
        <div className="loading loading-spinner loading-lg"></div>
      </div>
    );
  }

  // Don't render table until we have pagination info
  if (!paginationInfo) {
    return null;
  }

  return (
    <div className="flex flex-col p-4">
      {/* <div className="mb-4">
        <EventFilters />
      </div> */}
      
      <ServerPaginatedTable
        columns={columns}
        data={events}
        paginationInfo={paginationInfo}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
        title="Event History"
        bordered={false}
        gridView={true}
      />
    </div>
  );
};

export default EventHistory;