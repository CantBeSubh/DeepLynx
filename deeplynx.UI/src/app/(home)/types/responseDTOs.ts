export type ClassResponseDto = {
  id: number;
  name: string;
  description: string | null;
  uuid: string | null;
  projectid: number;
  lastUpdatedAt: string | null;
  lastUpdatedBy: string | null;
  isArchived: boolean;
  archivedat: string | null;
  createdby: string | null;
  createdat: string;
};

export type TokenResponseDto = {
  apiKey: string;
  apiSecret: string;
}

export type DataSourceResponseDto = {
  id: number;
  name: string;
  description: string | null;
  default: boolean;
  abbreviation: string | null;
  type: string | null;
  baseuri: string | null;
  config: Record<string, unknown> | null; // object | null
  projectid: number;
  lastUpdatedAt: string | null; // RFC 3339 or null
  lastUpdatedBy: string | null;
  isArchived: boolean;
  createdby: string | null;
  createdat: string;          // RFC 3339 date-time
  archivedat: string | null;  // RFC 3339 or null
};

export type RelatedRecordsResponseDto = {
  relatedRecordName: string;
  relatedRecordId: number;
  relatedRecordProjectId: number;
  relationshipName: string | null;
}

export type GroupResponseDto = {
  id: number | string;
  name: string;
  description?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived: boolean;
  organizationId: number | string;
  memberCount?: number;
}

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
  LastUpdatedAt?: Date;
  LastUpdateBy?: string;
  isArchived: boolean
  CreatedBy?: string;
  CreatedAt?: Date;
  ArchivedAt?: Date;
}

export type RecordResponseDto = {
  id: number | null;
  name: string;
  description?: string | null;
  uri?: string | null;
  properties?: unknown;
  objectStorageId?: number | null;
  originalId?: string | null;
  classId?: number | null;
  dataSourceId?: number | null;
  projectId?: number | null;
  lastUpdatedAt?: string;
  lastUpdatedBy?: string | null;
  isArchived?: boolean;
  fileType?: string | null;
  tags?: { id: number | null; name: string }[];
};

export type ObjectStorageResponseDto = {
  id: number | string;
  name: string;
  type: string;
  projectId: number | string;
  default: boolean;
  lastUpdatedAt: string;
  lastUpdatedBy: string;
  isArchived: boolean;
}

export type OrganizationResponseDto = {
  id: number | string;
  name: string;
  description?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived: boolean;
  defaultOrg?: boolean;
}

export type PermissionResponseDto = {
  id: number | string;
  name: string;
  description?: string | null;
  action: string;
  resource?: string | null;
  isDefault: boolean;
  labelId?: number | string;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived: boolean;
  projectId?: number | string;
  organizationId?: number | string;
}

export type ProjectMembersDto = {
  name: string;
  memberId?: number | null;
  email: string;
  role: string;
  roleId?: number | null;
  groupId?: number | null;
  projectId: number;
}

export type ProjectResponseDto = {
  id: number | string;
  name: string;
  description?: string | null;
  abbreviation?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived: boolean;
  organizationId: number | string;
};

export type ProjectStatResponseDto = {
  classes: number;
  records: number;
  datasources: number;
}

//which one
export type RoleResponseDto = {
  id: number;
  name: string;
  description: string | null;
  lastUpdatedAt: string;
  lastUpdatedBy: number;
  isArchived: boolean;
  projectId: number;
  organizationId: number;
}

export type TagResponseDto = {
  id: number;
  name: string;
  projectId: number;
  lastUpdatedAt?: string | null;
  lastUpdatedBy?: string | null;
  isArchived: boolean;
  archivedAt?: string | null;
};

export type UserResponseDto =
  {
    id: number;
    name: string;
    email: string;
    username: string;
    isSysAdmin: boolean;
    isArchived: boolean;
    isActive: boolean;
    role?: string;
  }

export type GraphResponseDto = {
  nodes: Array<{
    id: number;
    label: string;
    type: string;
  }>;
  links: Array<{
    source: number;
    target: number;
    relationshipId: number;
    relationshipName: string | null;
    edgeId: number;
  }>;
}

export type OauthApplicationResponseDto = {
  id: number;
  clientId: string;
  name: string;
  description?: string;
  callbackUrl: string;
  baseUrl?: string;
  appOwnerEmail?: string;
  isArchived: boolean;
  lastUpdatedAt: string;
  lastUpdatedBy?: number;
}

export type OauthApplicationSecureResponseDto = {
  name: string;
  clientId: string;
  clientSecretRaw: string;
}

export type PaginatedEventsResponseDto =
  {
    items: EventResponseDto[] | [];
    pageNumber: number;
    pageSize: number;
    maxPageSize: number;
    totalCount: number;
  };

export type EventResponseDto =
  {
    id: number;
    operation: string;
    entityType: string;
    entityId?: number | null;
    entityName: string;
    projectId?: number;
    projectName?: string;
    organizationId?: number | null;
    dataSourceId?: number | null;
    dataSourceName?: string | null;
    properties?: JSON | string | null;
    lastUpdatedAt?: string | null;
    lastUpdatedBy?: string | null;
    lastUpdatedByUserName?: string | null;
  };

export type EdgeResponseDto = {
  id: number;
  originId: number;
  destinationId: number;
  relationshipId?: number;
  dataSourceId: number;
  projectId: number;
  lastUpdatedAt: string;
  lastUpdatedBy?: number;
  isArchived: boolean;
};

export type RelationshipResponseDto = {
  id: number;
  name: string;
  description?: string;
  uuid?: string;
  projectId: number;
  lastUpdatedAt: string;
  lastUpdatedBy?: number;
  isArchived: boolean;
  originId?: number;
  destinationId?: number;
};

export type ProjectMemberResponseDto = {
  name: string;
  memberId?: number;
  email: string;
  role?: string;
  roleId?: number;
};