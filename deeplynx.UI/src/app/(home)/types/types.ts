
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
  mappingId?: string | null;
  dataSourceId?: number;
  dataSourceName?: string;
  projectId?: number;
  projectName?: string;
  tags: string;
  createdBy?: string | null;
  createdAt?: string | null;
  modifiedBy?: string | null;
  modifiedAt?: string | null;
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
  header?: string;
  data?: keyof T;
  sortable?: boolean;
  cell?: (row: T) => React.ReactNode
};

export type ProjectsList = {
  id?: string;
  name: string;
  description: string;
  lastViewed: string;
  createdAt: string;
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

