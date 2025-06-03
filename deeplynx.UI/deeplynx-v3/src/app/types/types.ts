
export type DataSourceTableRow = {
    name: string;
    country: string;
    adapterType: string;
    active: boolean;
    id: string;
    select?: boolean;
  };

export type FileViewerTableRow = {
    id: number;
    fileName: string;
    timeseries: boolean;
    fileSize: number;
    dateModified: string;
    select?: boolean;
  };

export type TableRow = DataSourceTableRow | FileViewerTableRow;

export type Column<T extends object> = {
    header: string;
    data?: keyof T;
    sortable?: boolean;
    cell?: (row: T) => React.ReactNode
  };

export type ProjectsList = {
    id?: string;
    name: string;
    description: string;
    lastViewed: string;
  }