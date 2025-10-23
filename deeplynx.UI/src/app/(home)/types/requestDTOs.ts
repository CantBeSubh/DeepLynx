export type CustomQueryRequestDto = {
  connector?: string | null;
  filter: string;
  operator: string;
  value: string;
  json?: string;
  jsonKey?: string;
  jsonValue?: string;
};

export type RelatedRecordsRequestDto = {
    recordId?: number;
    isOrigin?: boolean;
    page?: number;
    pageSize?: number;
    hideArchived?: boolean;
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