export type ClassResponseDto = {
  id: number;
  name: string;
  description: string | null;
  uuid: string | null;
  projectid: number;
  lastUpdatedAt: string | null;
  lastUpdatedBy: string | null;
  isArchived:boolean;
  archivedat: string | null;
  createdby: string | null;
  createdat: string;
};

export type DataSourceResponseDto = {
  id: number;
  name: string;
  description: string | null;
  default:boolean;
  abbreviation: string | null;
  type: string | null;
  baseuri: string | null;
  config: Record<string, unknown> | null; // object | null
  projectid: number;
  lastUpdatedAt: string | null; // RFC 3339 or null
  lastUpdatedBy: string | null;  
  isArchived:boolean;
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
  id: number|string;
  name: string;
  description?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
  organizationId: number | string;
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
  isArchived:boolean
  CreatedBy?: string;
  CreatedAt?: Date;
  ArchivedAt?: Date;
}

export type OrganizationResponseDto = {
  id: number|string;
  name: string;
  description?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
}

export type PermissionResponseDto = {
  id: number|string;
  name: string;
  description?: string | null;
  action:string;
  resource?:string|null;
  isHardcoded:boolean;
  labelId?:number|string;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
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
  abbreviation ?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
  organizationId: number | string;
};

export type RoleResponseDto =
{
roleId: number;
name: string;
description?: string;
}

export type TagResponseDto = {
  id: number;
  name: string;
  projectId: number;
  lastUpdatedAt?: string | null;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
  createdBy?: string | null;
  createdAt: string; // ISO date string from backend
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
}