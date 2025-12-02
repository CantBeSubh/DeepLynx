"use client";

import api from "./api";
import type {
  DataSourceResponseDto,
} from "../../(home)/types/responseDTOs";
import type {
  CreateDataSourceRequestDto,
  UpdateDataSourceRequestDto,
} from "../../(home)/types/requestDTOs";

/* ========================================================================== */
/*                          Project-scoped Data Sources                        */
/* ========================================================================== */

/**
 * Create a new data source for a project.
 *
 * POST /projects/{projectId}/datasources
 */
export const createDataSource = async (
  projectId: number,
  dto: CreateDataSourceRequestDto
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.post(`/projects/${projectId}/datasources`, dto);
    return res.data;
  } catch (error) {
    console.error("Error creating data source:", error);
    throw error;
  }
};

/**
 * Update a data source for a specific project.
 *
 * PUT /projects/{projectId}/datasources/{dataSourceId}
 */
export const updateProjectDataSource = async (
  projectId: number,
  dataSourceId: number,
  dto: UpdateDataSourceRequestDto
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.put(
      `/projects/${projectId}/datasources/${dataSourceId}`,
      dto
    );
    return res.data;
  } catch (error) {
    console.error(
      `Error updating project data source ${dataSourceId} for project ${projectId}:`,
      error
    );
    throw error;
  }
};

/**
 * Archive or unarchive a data source for a specific project.
 *
 * PATCH /projects/{projectId}/datasources/{dataSourceId}?archive=true|false
 */
export const archiveProjectDataSource = async (
  projectId: number,
  dataSourceId: number,
  archive: boolean
): Promise<{ message: string }> => {
  try {
    const res = await api.patch(
      `/projects/${projectId}/datasources/${dataSourceId}`,
      null,
      { params: { archive } }
    );

    return res.data;
  } catch (error) {
    console.error(
      `Error ${
        archive ? "archiving" : "unarchiving"
      } project data source ${dataSourceId}:`,
      error
    );
    throw error;
  }
};

/**
 * Set or unset a data source as the default for a specific project.
 *
 * PATCH /projects/{projectId}/datasources/{dataSourceId}/default?isDefault=true|false
 */
export const setDefaultDataSourceForProject = async (
  projectId: number,
  dataSourceId: number,
  isDefault: boolean = true
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.patch(
      `/projects/${projectId}/datasources/${dataSourceId}/default`,
      null,
      { params: { isDefault } }
    );

    return res.data;
  } catch (error) {
    console.error(
      `Error ${
        isDefault ? "setting" : "unsetting"
      } default data source ${dataSourceId} for project ${projectId}:`,
      error
    );
    throw error;
  }
};

/**
 * Get the default data source for a specific project.
 *
 * GET /projects/{projectId}/datasources/default
 */
export const getDefaultDataSourceForProject = async (
  projectId: number
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(`/projects/${projectId}/datasources/default`);
    return res.data;
  } catch (error) {
    console.error(
      `Error getting default data source for project ${projectId}:`,
      error
    );
    throw error;
  }
};

/**
 * Get record count for a project, optionally filtered by data source.
 *
 * GET /organizations/{organizationId}/projects/{projectId}/records/count
 */
export const getRecordCountForDataSource = async (
  organizationId: number,
  projectId: number,
  dataSourceId?: number,
  hideArchived: boolean = true
): Promise<number | null> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/records/count`,
      {
        params: {
          dataSourceId,
          hideArchived,
        },
      }
    );

    // Backend may return `null` when there are no records
    return res.data as number | null;
  } catch (error) {
    console.error(
      `Error getting record count for project ${projectId}${
        dataSourceId ? ` and data source ${dataSourceId}` : ""
      }:`,
      error
    );
    throw error;
  }
};

/**
 * Get all data sources for a project.
 *
 * GET /projects/{projectId}/datasources?hideArchived=true|false
 */
export const getAllDataSources = async (
  projectId: number,
  hideArchived: boolean = true
): Promise<DataSourceResponseDto[]> => {
  try {
    const res = await api.get(`/projects/${projectId}/datasources`, {
      params: { hideArchived },
    });
    return res.data;
  } catch (error) {
    console.error("Error getting all data sources:", error);
    throw error;
  }
};

/**
 * Get a specific data source (project-scoped).
 *
 * GET /projects/{projectId}/datasources/{dataSourceId}
 */
export const getDataSource = async (
  projectId: number,
  dataSourceId: number,
  hideArchived: boolean = true
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(
      `/projects/${projectId}/datasources/${dataSourceId}`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Update a data source (project-scoped convenience alias).
 *
 * PUT /projects/{projectId}/datasources/{dataSourceId}
 */
export const updateDataSource = async (
  projectId: number,
  dataSourceId: number,
  dto: UpdateDataSourceRequestDto
): Promise<DataSourceResponseDto> => {
  // For backwards compatibility; delegates to updateProjectDataSource
  return updateProjectDataSource(projectId, dataSourceId, dto);
};

/**
 * Delete a data source (project-scoped).
 *
 * DELETE /projects/{projectId}/datasources/{dataSourceId}
 */
export const deleteDataSource = async (
  projectId: number,
  dataSourceId: number
): Promise<{ message: string }> => {
  try {
    const res = await api.delete(
      `/projects/${projectId}/datasources/${dataSourceId}`
    );
    return res.data;
  } catch (error) {
    console.error(`Error deleting data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Archive or unarchive a data source (project-scoped convenience alias).
 *
 * PATCH /projects/{projectId}/datasources/{dataSourceId}?archive=true|false
 */
export const archiveDataSource = async (
  projectId: number,
  dataSourceId: number,
  archive: boolean
): Promise<{ message: string }> => {
  // For backwards compatibility; delegates to archiveProjectDataSource
  return archiveProjectDataSource(projectId, dataSourceId, archive);
};

/**
 * Set default data source for a project (project-scoped convenience alias).
 *
 * PATCH /projects/{projectId}/datasources/{dataSourceId}/default?isDefault=true|false
 */
export const setDefaultDataSource = async (
  projectId: number,
  dataSourceId: number,
  isDefault: boolean = true
): Promise<DataSourceResponseDto> => {
  // For backwards compatibility; delegates to setDefaultDataSourceForProject
  return setDefaultDataSourceForProject(projectId, dataSourceId, isDefault);
};

/* ========================================================================== */
/*                       Organization-scoped Data Sources                      */
/* ========================================================================== */

/**
 * Get all data sources for an organization.
 *
 * GET /organizations/{organizationId}/datasources?projectIds=&hideArchived=
 */
export const getAllDataSourcesOrg = async (
  organizationId: number,
  projectIds?: number[],
  hideArchived: boolean = true
): Promise<DataSourceResponseDto[]> => {
  try {
    const res = await api.get(`/organizations/${organizationId}/datasources`, {
      params: { projectIds, hideArchived },
    });
    return res.data;
  } catch (error) {
    console.error("Error getting all data sources for organization:", error);
    throw error;
  }
};

/**
 * Get a specific data source at organization level.
 *
 * GET /organizations/{organizationId}/datasources/{dataSourceId}
 */
export const getDataSourceOrg = async (
  organizationId: number,
  dataSourceId: number,
  hideArchived: boolean = true
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/datasources/${dataSourceId}`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error(
      `Error getting data source ${dataSourceId} for organization:`,
      error
    );
    throw error;
  }
};

/**
 * Get default data source at organization level.
 *
 * GET /organizations/{organizationId}/datasources/default
 */
export const getDefaultDataSourceOrg = async (
  organizationId: number
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/datasources/default`
    );
    return res.data;
  } catch (error) {
    console.error(
      "Error getting default data source for organization:",
      error
    );
    throw error;
  }
};

/**
 * Update a data source at organization level.
 *
 * PUT /organizations/{organizationId}/datasources/{dataSourceId}
 */
export const updateDataSourceOrg = async (
  organizationId: number,
  dataSourceId: number,
  dto: UpdateDataSourceRequestDto
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.put(
      `/organizations/${organizationId}/datasources/${dataSourceId}`,
      dto
    );
    return res.data;
  } catch (error) {
    console.error(
      `Error updating data source ${dataSourceId} for organization:`,
      error
    );
    throw error;
  }
};

/**
 * Delete a data source at organization level.
 *
 * DELETE /organizations/{organizationId}/datasources/{dataSourceId}
 */
export const deleteDataSourceOrg = async (
  organizationId: number,
  dataSourceId: number
): Promise<{ message: string }> => {
  try {
    const res = await api.delete(
      `/organizations/${organizationId}/datasources/${dataSourceId}`
    );
    return res.data;
  } catch (error) {
    console.error(
      `Error deleting data source ${dataSourceId} for organization:`,
      error
    );
    throw error;
  }
};

/**
 * Archive or unarchive a data source at organization level.
 *
 * PATCH /organizations/{organizationId}/datasources/{dataSourceId}?archive=true|false
 */
export const archiveDataSourceOrg = async (
  organizationId: number,
  dataSourceId: number,
  archive: boolean
): Promise<{ message: string }> => {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/datasources/${dataSourceId}`,
      null,
      { params: { archive } }
    );
    return res.data;
  } catch (error) {
    console.error(
      `Error ${
        archive ? "archiving" : "unarchiving"
      } data source ${dataSourceId} for organization:`,
      error
    );
    throw error;
  }
};

/**
 * Set default data source at organization level.
 *
 * PATCH /organizations/{organizationId}/datasources/{dataSourceId}/default
 */
export const setDefaultDataSourceOrg = async (
  organizationId: number,
  dataSourceId: number
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/datasources/${dataSourceId}/default`
    );
    return res.data;
  } catch (error) {
    console.error(
      `Error setting default data source ${dataSourceId} for organization:`,
      error
    );
    throw error;
  }
};
