import React, { ReactNode } from 'react';
import { CustomQueryRequestDto } from './requestDTOs';
//For dummy data
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

export interface Group {
  id: number;
  name: string;
  description?: string;
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

//record_service 
export type RecordRow = {
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
export type UpdateRecordPayload = {
  uri?: string | null;
  properties?: Record<string, unknown>;
  original_id?: string | null;
  name?: string | null;
  class_id?: number | null;
  class_name?: string | null;
  description?: string | null;
};

//DatePicker
export type DatePickerQuery = {
    id: string;
    connector?: string;
    filter?: string;
    operator?: string;
    value?: string; // you can store the combined timestamp here if you want
};

//NewFileUploadCard
export type FileMetadata = {
  name: string;
  description: string;
  isTimeSeries: boolean;
  updateAction?: "merge" | "overwrite";
};

//RecentRecordsCard
export type RecentRecord = {
  id: number;
  name: string;
  className: string;
  createdAt: string;
  lastUpdatedAt: string;
  dataSourceName: string;
  projectName: string;
  projectId: number;
};

export type UserResponseDto =
  {
    id: number;
    name?: string;
    email?: string;
    username?: string;
    ssoId?: string;
    isSysAdmin?: boolean;
    isArchined?: boolean;
    isActive?: boolean;
    projectId?: number;
  }

  export type UserRequestDto =
  {
    name: string;
    username: string;
    ssoId: string;
    isArchived: boolean | null;
    projectId: number;
    isActive: boolean | null;
  }

  export type RoleResponseDto =
  {
    id: number;
    name?: string;
    description?: string | null;
    lastUpdatedAt?: string | Date;
    lastUpdatedBy?: string | null;
    isArchieved?: boolean;
    projectId?: number;
    organizationId?: number;
    roleId?: number;
  }

  export type ProjectResponseDto =
  {
    id: number;
    name?: string;
    description?: string | null;
    abbreviation?: string | null;
    lastUpdatedAt?: string | Date;
    lastUpdatedBy?: string | null;
    isArchieved?: boolean;
    projectId?: number;
    organizationId?: number;
    roleId?: number;
    userId?: number;
  }

  export type PermissionResponseDto =
  {
    id: number;
    name?: string;
    description?: string | null;
    action?: string;
    resource?: string | null;
    isDefault?: boolean;
    labelId?: number;
    lastUpdatedAt?: string | Date;
    lastUpdatedBy?: string | null;
    isArchieved?: boolean;
    projectId?: number;
    organizationId?: number;
    roleId?: number;
    userId?: number;
  }

  export type CreateRoleRequestDto =
  {
    name: string;
    description?: string | null;
    projectId?: number;
    organizationId?: number;
  }

  export type PermissionRequestDto =
  {
    name: string;
    description?: string | null;
    projectId?: number;
    organizationId?: number;
  }
//Widgets
export type WidgetType =
  | "DataOverview"
  | "Links"
  | "Graph"
  | "RecentActivity"
  | "ProjectOverview"
  | "TeamMembers";

export type QueryBuilderQuery = {
  id: string;
  query: CustomQueryRequestDto;
};

//Uploads
export type UploadType = "new" | "version" | "properties" | "";

export type ExistingFile = {
  id: string;
  name: string;
  alias?: string;
  description?: string;
  lastUpdate?: string;
  updatedBy?: string;
  tags?: string[];
  dataSource?: string;
  propertiesSources?: string;
  timeSeries?: boolean;
  image?: string;
};

export type RecentUploadIcon = "arrows" | "link" | "stack";

export type RecentUpload = {
  id: string;
  name: string;
  avatar: string;
  file: string;
  icon: RecentUploadIcon;
};

//file_upload_services
export type UploadFileArgs = {
  projectId: number | string;
  dataSourceId: number | string;
  objectStorageId: number | string;
  file: File;

  // optional metadata
  name?: string;
  description?: string;
  properties?: unknown;
  tags?: string[];
  originalId?: string;
  classId?: number | string;
};

//notification_services
/** ---- Types ---- */
export type SendEmailResponse = {
  message: string;
};

//record_services
export type CreateRecordPayload = {
  name: string;
  original_id: string;
  description: string;

  properties: Record<string, unknown>;

  class_id?: number | null;
  object_storage_id?: number | null;
  uri?: string | null;
  class_name?: string | null;
  tags?: string[] | null;
  sensitivity_labels?: string[] | null;
};

export type Role = {
  id: number;
  name: string;
  description: string | null;
  lastUpdatedAt: string;
  lastUpdatedBy: string | null;
  isArchived: boolean;
  projectId: number;
  organizationId: number | null;
}
