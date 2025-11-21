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

export type CreateClassRequestDto = {
  name: string;
  description: string;
  uuid?: string;
}

export type UpdateClassRequestDto = {
  name: string;
  description: string;
  uuid?: string;
}

export type CreateDataSourceRequestDto = {
  name: string;
  description?: string;
  abbreviation?: string;
  type?: string;
  baseUri?: string;
  config?: string;
};

export type UpdateDataSourceRequestDto = {
  name?: string;
  description?: string;
  abbreviation?: string;
  type?: string;
  baseUri?: string;
  config?: string;
};

export type CreateEdgeRequestDto = {
  origin_id?: number;
  destination_id?: number;
  relationship_id?: number;
  relationship_name?: string;
  origin_oid?: string;
  destination_oid?: string;
};

export type UpdateEdgeRequestDto = {
  origin_id?: number;
  destination_id?: number;
  name?: string;
  relationshipId?: number;
};