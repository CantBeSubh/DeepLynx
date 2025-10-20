"use client";

import GenericTable from "../components/GenericTable";
import { Column } from "@/app/(home)/types/types";
import { useMemo } from "react";

// Define the Event type based on the C# model
interface Event {
  id: number;
  operation: string;
  entityType: string;
  entityId: number | null;
  projectId: number | null;
  organizationId: number | null;
  dataSourceId: number | null;
  properties: string;
  lastUpdatedBy: string | null;
  lastUpdatedAt: Date;
  // Additional computed fields
  project?: string;
  entity?: string;
  summary?: React.ReactNode;
  dataSource?: string;
}

// Mock lookup data
const projects: Record<number, string> = {
  1: "RocketBooster",
  2: "SatelliteTracking",
  3: "PropulsionSystem",
};

const dataSources: Record<number, string> = {
  5: "Manufacturing API",
  7: "Sensor Data Stream",
  9: "Quality Control CSV",
  10: "Legacy System Import",
  11: "Real-time Telemetry",
};

const entities: Record<string, Record<number, string>> = {
  Node: {
    101: "Temperature Sensor A1",
    103: "Pressure Valve B2",
    104: "Flow Meter C3",
  },
  Edge: {
    202: "Sensor-to-Controller Link",
    205: "Valve-to-Pump Connection",
  },
  DataSource: {
    8: "Manufacturing CSV Import",
  },
  Class: {
    1: "Equipment",
    2: "Sensor",
    3: "Actuator",
  },
  Record: {
    301: "Inspection Report #2024-10",
    302: "Maintenance Log #455",
    303: "Calibration Record #789",
  },
};

const EventHistory = () => {
  // Helper function to get project name
  const getProjectName = (projectId: number | null): string => {
    if (!projectId) return "N/A";
    return projects[projectId] || `Project ${projectId}`;
  };

  // Helper function to get data source name
  const getDataSourceName = (dataSourceId: number | null): string => {
    if (!dataSourceId) return "N/A";
    return dataSources[dataSourceId] || `Data Source ${dataSourceId}`;
  };

  // Helper function to get entity name
  const getEntityName = (entityType: string, entityId: number | null): string => {
    if (!entityId) return "N/A";
    return entities[entityType]?.[entityId] || `${entityType} ${entityId}`;
  };

  // Helper function to generate summary JSX
  const generateSummary = (event: Event): React.ReactNode => {
    const user = event.lastUpdatedBy || "System";
    const operation = event.operation;
    const operationText = operation === "CREATE" ? "Created" : operation === "UPDATE" ? "Updated" : "Deleted";
    const article = operation === "UPDATE" ? "an" : "a";
    const entityName = getEntityName(event.entityType, event.entityId);
    const projectName = getProjectName(event.projectId);
    
    return (
      <span className="text-sm">
        User {user} <span className="font-bold">{operationText}</span> {article} <span className="font-bold">{event.entityType}</span> {entityName} in Project <span className="font-bold">{projectName}</span>
      </span>
    );
  };

  // Mock data for events
  const mockEvents: Event[] = useMemo(() => {
    const rawEvents: Event[] = [
      {
        id: 1,
        operation: "CREATE",
        entityType: "Node",
        entityId: 101,
        projectId: 1,
        organizationId: 1,
        dataSourceId: 5,
        properties: JSON.stringify({ name: "Temperature Sensor A1", type: "equipment" }),
        lastUpdatedBy: "john.doe@inl.gov",
        lastUpdatedAt: new Date("2024-10-15T10:30:00"),
      },
      {
        id: 2,
        operation: "UPDATE",
        entityType: "Edge",
        entityId: 202,
        projectId: 1,
        organizationId: 1,
        dataSourceId: 5,
        properties: JSON.stringify({ relationship: "contains", weight: 1.5 }),
        lastUpdatedBy: "jane.smith@inl.gov",
        lastUpdatedAt: new Date("2024-10-16T14:22:00"),
      },
      {
        id: 3,
        operation: "DELETE",
        entityType: "Node",
        entityId: 103,
        projectId: 2,
        organizationId: 1,
        dataSourceId: 7,
        properties: JSON.stringify({ archived: true, reason: "duplicate" }),
        lastUpdatedBy: "admin@inl.gov",
        lastUpdatedAt: new Date("2024-10-17T09:15:00"),
      },
      {
        id: 4,
        operation: "CREATE",
        entityType: "DataSource",
        entityId: 8,
        projectId: 2,
        organizationId: 1,
        dataSourceId: null,
        properties: JSON.stringify({ name: "Manufacturing CSV Import", active: true }),
        lastUpdatedBy: "system",
        lastUpdatedAt: new Date("2024-10-18T11:45:00"),
      },
      {
        id: 5,
        operation: "UPDATE",
        entityType: "Node",
        entityId: 104,
        projectId: 1,
        organizationId: 1,
        dataSourceId: 5,
        properties: JSON.stringify({ status: "active", priority: "high" }),
        lastUpdatedBy: "keaton.flake@inl.gov",
        lastUpdatedAt: new Date("2024-10-19T16:30:00"),
      },
      {
        id: 6,
        operation: "CREATE",
        entityType: "Edge",
        entityId: 205,
        projectId: 3,
        organizationId: 2,
        dataSourceId: 9,
        properties: JSON.stringify({ relationship: "parent_of" }),
        lastUpdatedBy: "jane.smith@inl.gov",
        lastUpdatedAt: new Date("2024-10-20T08:12:00"),
      },
      {
        id: 7,
        operation: "CREATE",
        entityType: "Record",
        entityId: 301,
        projectId: 1,
        organizationId: 1,
        dataSourceId: 5,
        properties: JSON.stringify({ type: "inspection", status: "completed" }),
        lastUpdatedBy: "keaton.flake@inl.gov",
        lastUpdatedAt: new Date("2024-10-20T09:45:00"),
      },
      {
        id: 8,
        operation: "UPDATE",
        entityType: "Class",
        entityId: 1,
        projectId: 2,
        organizationId: 1,
        dataSourceId: 7,
        properties: JSON.stringify({ description: "Updated equipment classification" }),
        lastUpdatedBy: "admin@inl.gov",
        lastUpdatedAt: new Date("2024-10-20T11:20:00"),
      },
      {
        id: 9,
        operation: "DELETE",
        entityType: "Record",
        entityId: 302,
        projectId: 3,
        organizationId: 2,
        dataSourceId: 9,
        properties: JSON.stringify({ reason: "outdated" }),
        lastUpdatedBy: "john.doe@inl.gov",
        lastUpdatedAt: new Date("2024-10-20T13:05:00"),
      },
    ];

    // Add computed fields
    return rawEvents.map(event => ({
      ...event,
      project: getProjectName(event.projectId),
      entity: getEntityName(event.entityType, event.entityId),
      summary: generateSummary(event),
      dataSource: getDataSourceName(event.dataSourceId),
    }));
  }, []);

  // Define columns for the table
  const columns: Column<Event>[] = useMemo(() => [
    {
      header: "Last Updated At",
      data: "lastUpdatedAt",
      sortable: true,
      cell: (row) => row.lastUpdatedAt.toLocaleString(),
    },
    {
      header: "Summary",
      data: "summary",
      sortable: true,
      cell: (row) => row.summary,
    },
    {
      header: "Last Updated By",
      data: "lastUpdatedBy",
      sortable: true,
      cell: (row) => row.lastUpdatedBy ?? "Unknown",
    },
    {
      header: "Project",
      data: "project",
      sortable: true,
      cell: (row) => row.project ?? "N/A",
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
      header: "Entity",
      data: "entity",
      sortable: true,
      cell: (row) => row.entity ?? "N/A",
    },
    {
      header: "Data Source",
      data: "dataSource",
      sortable: true,
      cell: (row) => row.dataSource ?? "N/A",
    }
  ], []);

  return (
    <div className="p-4">
      <GenericTable
        columns={columns}
        data={mockEvents}
        title="Event History"
        filterPlaceholder="Search events..."
        enablePagination={true}
        rowsPerPage={10}
        bordered={true}
        searchBar={true}
        gridView={true}
      />
    </div>
  );
};

export default EventHistory;