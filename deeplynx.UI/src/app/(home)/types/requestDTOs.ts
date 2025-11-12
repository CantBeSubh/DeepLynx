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

export type CreateOrganizationRequestDto = {
  name: string;
  description?: string;
}

export type UpdateOrganizationRequestDto = {
  name: string;
  description?: string;
}

export type CreateOauthApplicationRequestDto = {
  name: string;
  description?: string;
  callbackUrl: string;
  baseUrl?: string;
  appOwnerEmail?: string;
}

export type UpdateOauthApplicationRequestDto = {
  name?: string;
  description?: string;
  callbackUrl?: string;
  baseUrl?: string;
  appOwnerEmail?: string;
}

export type CreateObjectStorageRequestDto = {
  name: string;
  config: string;
}

export type UpdateObjectStorageRequestDto = {
  name: string;
}