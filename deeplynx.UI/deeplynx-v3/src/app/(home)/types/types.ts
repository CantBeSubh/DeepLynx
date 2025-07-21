
export type DataSourceTableRow = {
    name: string;
    country: string;
    adapterType: string;
    active: boolean;
    id: string | number;
    select?: boolean;
  };

export type FileViewerTableRow = {
  id: number;
  uri?: string | null;
  properties?: string;
  originalId?: string;
  name?: string;
  classId?: number;
  className?: string;
  mappingId?: string | null;
  dataSourceId?: number;
  dataSourceName?: string;
  projectId?: number;
  projectName?: string;
  tags: string[];
  createdBy?: string | null;
  createdAt?: string;
  modifiedBy?: string | null;
  modifiedAt?: string | null;
  archivedAt?: string | null;
  desc?: string;
  fileType: string;
  timeseries?: boolean;
  fileSize?: number;
  select?: boolean;
  associatedRecords?: string[];

    // id: number;
    // fileName: string;
    // fileDescription: string;
    // fileType: string;
    // timeseries: boolean;
    // fileSize: number;
    // dateModified: string;
    // select?: boolean;
    // tags: string[];
    // lastEdit: string;
    // associatedRecords?: string[];
  };

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