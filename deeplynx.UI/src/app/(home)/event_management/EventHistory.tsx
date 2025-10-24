"use client";

import GenericTable from "../components/GenericTable";
import { Column } from "@/app/(home)/types/types";
import { useEffect, useMemo, useState } from "react";
import { getAllEventsByUser } from "@/app/lib/event_services.client";

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
  // Additional computed fields
  summary?: React.ReactNode;
  dataSource?: string;
};

const EventHistory = () => {

  const [events, setEvents] = useState<Event[]>([]);

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        let fetchedEvents: Event[] = await getAllEventsByUser();
        setEvents(fetchedEvents);
        console.log(fetchedEvents); // ✅ Log the fetched data, not the state
      } 
      catch (e) {
        console.error('Failed to fetch events:', e);
      }
    };
  
    fetchEvents();
  }, []);

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
  const spaced = type.replace(/_/g, " "); // Replace all underscores with spaces
  const capitalized = spaced
    .split(" ")
    .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
    .join(" ");
  
  return capitalized;
}

  // Helper function to generate summary JSX
  // const generateSummary = (event: Event): React.ReactNode => {
  //   const user = event.lastUpdatedBy || "System";
  //   const operation = event.operation;
  //   const operationText = operation === "CREATE" ? "Created" : operation === "UPDATE" ? "Updated" : "Deleted";
  //   const article = operation === "UPDATE" ? "an" : "a";
  //   const entityName = event.entityName
  //   const projectName = event.projectName
    
    // return (
    //   <span className="text-sm">
    //     User {user} <span className="font-bold">{operationText}</span> {article} <span className="font-bold">{event.entityType}</span> {entityName} in Project <span className="font-bold">{projectName}</span>
    //   </span>
    // );
  // };

  // Define columns for the table
  const columns: Column<Event>[] = useMemo(() => [
    {
      header: "Event Occured",
      data: "lastUpdatedAt",
      sortable: true,
      cell: (row) => formatDate(row.lastUpdatedAt.toString()),
    },
    // {
    //   header: "Summary",
    //   data: "summary",
    //   sortable: true,
    //   cell: (row) => row.summary,
    // },
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
      cell: (row) => row.dataSource ?? "N/A",
    }
  ], []);

  return (
    <div className="p-4">
      <GenericTable
        columns={columns}
        data={events}
        title="Event History"
        filterPlaceholder="Search events..."
        enablePagination={true}
        rowsPerPage={10}
        bordered={false}
        searchBar={true}
        gridView={true}
      />
    </div>
  );
};

export default EventHistory;