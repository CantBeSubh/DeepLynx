import React, { ReactNode } from 'react';

export type DataSourceTableRow = {
  name: string;
  country: string;
  adapterType: string;
  active: boolean;
  id: string | number;
  select?: boolean;
};

export type Tag = {
  id?: number;
  name: string
}

// TODO: change Tag[] to string[] and figure out how to display
export type FileViewerTableRow = {
  id: number;
  uri?: string | null;
  name?: string;
  properties?: string;
  originalId?: string;
  classId?: number;
  className?: string;
  dataSourceId?: number;
  dataSourceName?: string;
  projectId?: number;
  projectName?: string;
  tags: string;
  archivedAt?: string | null;
  lastUpdatedAt?: string;
  description?: string;
  fileType: string;
  timeseries?: boolean;
  fileSize?: number;
  select?: boolean;
  associatedRecords?: string[];
};

export type Tags = {
  id: string;
  name: string;
}

export type TableRow = DataSourceTableRow | FileViewerTableRow;

export type Column<T extends object> = {
  accessor?: string;
  header?: string | ReactNode;
  data?: keyof T;
  sortable?: boolean;
  cell?: (row: T, index: number) => React.ReactNode
};

export type ProjectsList = {
  id?: string;
  name: string;
  description: string;
  lastUpdatedAt?: Date;
};

export type PopularTable = {
  id: number;
  name: string;
  image: string;
  nickname: string;
  visibility: string;
};

export type MySearchsTable = {
  id: number;
  name: string;
  filters: string[];
  createdAt: string;
  sortable?: boolean;
}

export type ProjectMembersTable = {
  projectId: number;
  name: string;
  email: string;
  role: string;
  userId?: number | null;
  groupId?: number | null;
}

export type SystemUsersTable = {
  id: number;
  name: string;
  email: string;
  role: string;
}

export type SystemGroupsTable = {
  id: number | null;
  name: string;
  description?: string | null;
}

export interface Group {
  id: number;
  name: string;
  description?: string;
}

export type SystemOrgsTable = {
  id: number;
  name: string;
  description: string;
}

export type MyRolesTable = {
  id: number;
  role: string;
  description: string;
}

export type ProjectPermissionsTable = {
  id: number;
  role: string;
  description: string;
}

export type UserPermissionsTable = {
  id: number;
  role: string;
  description: string;
}

export type TeamMember = {
  id: number;
  name: string;
  image: string;
  nickname: string;
  visibility: string;
  role: string;
  lastLogin: string;
};

export type ClassResponseDto = {
  id: number;
  name: string;
  description: string | null;
  uuid: string | null;
  projectid: number;
  createdby: string | null;
  createdat: string;
  modifiedby: string | null;
  modifiedat: string | null;
  archivedat: string | null;
};

export type DataSourceResponseDto = {
  id: number;
  name: string;
  description: string | null;
  abbreviation: string | null;
  type: string | null;
  baseuri: string | null;
  config: Record<string, unknown> | null; // object | null
  projectid: number;
  createdby: string | null;
  createdat: string;          // RFC 3339 date-time
  modifiedby: string | null;
  modifiedat: string | null;  // RFC 3339 or null
  archivedat: string | null;  // RFC 3339 or null
};

export type TagResponseDto = {
  id: number;
  name: string;
  projectId: number;
  createdBy?: string | null;
  createdAt: string; // ISO date string from backend
  modifiedBy?: string | null;
  modifiedAt?: string | null;
  archivedAt?: string | null;
};

export type CustomQueryRequestDto = {
  connector?: string | null;
  filter: string;
  operator: string;
  value: string;
  json?: string;
  jsonKey?: string;
  jsonValue?: string;
};

export type CustomQueryRequestDtoJson = {
  connector: string | null;
  filter: string;
  operator: string;
  value: string;
  json?: JSON;
};

export type HistoricalRecordResponseDto = {
  Id?: number;
  Uri?: string;
  Properties: string;
  OriginalId?: string;
  Name?: string;
  Description?: string;
  ClassId?: number;
  ClassName?: string;
  DataSourceId?: number;
  DataSourceName?: string;
  ObjectStorageId?: number;
  ObjectStorageName?: string;
  ProjectId: number;
  ProjectName: string;
  Tags?: string;
  CreatedBy?: string;
  CreatedAt?: Date;
  ModifiedBy?: string;
  ModifiedAt?: Date;
  ArchivedAt?: Date;
  LastUpdatedAt?: Date;
}

export type UserResponseDto =
  {
    Id: number;
    Name: string;
    Email: string;
    Username: string;
    SsoId: string;
    IsSysAdmin: boolean;
    IsArchined: boolean;
    isActive: boolean;
  }