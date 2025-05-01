
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

export type Column = {
    header: string;
    accessor: string;
    cell?: (row: TableRow) => React.ReactNode
  };