"use client";

import {
  CreateDataSourceRequestDto,
  UpdateDataSourceRequestDto,
} from "@/app/(home)/types/requestDTOs";
import { DataSourceResponseDto } from "@/app/(home)/types/responseDTOs";
import api from "./api";

/* -------------------------------------------------------------------------- */
/*                     Project-scoped Data Source Services                     */
/* -------------------------------------------------------------------------- */

/**
 * Create a new data source for a project
 * @param projectId - The ID of the project to which the data source belongs
 * @param dto - The data source creation request DTO
 * @returns Promise with DataSourceResponseDto
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
 * Update a data source for a specific project
 * (project-scoped endpoint)
 * @param projectId - The ID of the project to which the data source belongs
 * @param dataSourceId - The ID of the data source to update
 * @param dto - The data source update request DTO
 * @returns Promise with DataSourceResponseDto
 *
 * Matches docs:
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
 * Archive or unarchive a data source for a specific project
 * @param projectId - The ID of the project to which the data source belongs
 * @param dataSourceId - The ID of the data source to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export const archiveProjectDataSource = async (
  projectId: number,
  dataSourceId: number,
  archive: boolean
): Promise<{ message: string }> => {
  try {
    const res = await api.patch(
      `/projects/${projectId}/datasources/${dataSourceId}`,
      null, // no request body per docs
      { params: { archive } } // ?archive=true|false
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
 * Set or unset a data source as the default for a specific project
 * (project-scoped endpoint)
 * @param projectId - The ID of the project to which the data source belongs
 * @param dataSourceId - The ID of the data source to set/unset as default
 * @param isDefault - True to set as default (default: true), false to unset
 * @returns Promise with the updated DataSourceResponseDto
 *
 * Matches docs:
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
      null, // no body per docs
      { params: { isDefault } } // ?isDefault=true|false
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
 * Get the default data source for a specific project
 * (project-scoped endpoint)
 * @param projectId - The ID of the project to which the data source belongs
 * @returns Promise with DataSourceResponseDto
 *
 * GET /projects/{projectId}/datasources/default
 */
export const getDefaultDataSourceForProject = async (
  projectId: number
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(`/projects/${projectId}/datasources/default`);
    console.log(
      "🌐 [service] getDefaultDataSourceForProject response:",
      res.data
    );
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
 * Get record count for a project, optionally filtered by data source
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dataSourceId - Optional ID of the data source to filter by
 * @param hideArchived - Flag indicating whether to hide archived records (default: true)
 * @returns Promise with the record count (may be null if no records)
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


/* -------------------------------------------------------------------------- */
/*                  Organization-scoped Data Source Services                  */
/* -------------------------------------------------------------------------- */

/**
 * Get all data sources for an organization
 * @param organizationId - The ID of the organization
 * @param projectIds - Optional array of project IDs to filter by
 * @param hideArchived - Flag to hide archived data sources (default: true)
 * @returns Promise with array of DataSourceResponseDto
 */
export const getAllDataSources = async (
  organizationId: number,
  projectIds?: number[],
  hideArchived: boolean = true
): Promise<DataSourceResponseDto[]> => {
  try {
    const res = await api.get(`/organizations/${organizationId}/datasources`, {
      params: { projectIds, hideArchived },
    });
    console.log("Get all data sources", res);
    return res.data;
  } catch (error) {
    console.error("Error getting all data sources:", error);
    throw error;
  }
};

/**
 * Get a specific data source
 * @param organizationId - The ID of the organization
 * @param dataSourceId - The ID of the data source
 * @param hideArchived - Flag to hide archived data sources (default: true)
 * @returns Promise with DataSourceResponseDto
 */
export const getDataSource = async (
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
    console.error(`Error getting data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Get the default data source for an organization-scoped query
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @returns Promise with DataSourceResponseDto
 *
 * GET /organizations/{organizationId}/datasources/default?projectId={projectId}
 */
export const getDefaultDataSource = async (
  organizationId: number,
  projectId: number
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/datasources/default`,
      { params: { projectId } }
    );
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
 * Update a data source (organization-scoped)
 * @param organizationId - The ID of the organization
 * @param dataSourceId - The ID of the data source to update
 * @param dto - The data source update request DTO
 * @returns Promise with DataSourceResponseDto
 *
 * PUT /organizations/{organizationId}/datasources/{dataSourceId}
 */
export const updateDataSource = async (
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
    console.error(`Error updating data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Delete a data source
 * @param organizationId - The ID of the organization
 * @param dataSourceId - The ID of the data source to delete
 * @returns Promise with success message
 */
export const deleteDataSource = async (
  organizationId: number,
  dataSourceId: number
): Promise<{ message: string }> => {
  try {
    const res = await api.delete(
      `/organizations/${organizationId}/datasources/${dataSourceId}`
    );
    return res.data;
  } catch (error) {
    console.error(`Error deleting data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Archive or unarchive a data source (organization-scoped)
 * @param organizationId - The ID of the organization
 * @param dataSourceId - The ID of the data source to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export const archiveDataSource = async (
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
      `Error ${archive ? "archiving" : "unarchiving"} data source ${
        dataSourceId
      }:`,
      error
    );
    throw error;
  }
};

/**
 * Set a data source as the default for a project (organization-scoped endpoint)
 * @param organizationId - The ID of the organization
 * @param dataSourceId - The ID of the data source to set as default
 * @param projectId - The ID of the project
 * @param isDefault - True to set as default (default: true)
 * @returns Promise with DataSourceResponseDto
 *
 * PATCH /organizations/{organizationId}/datasources/{dataSourceId}/default?projectId={projectId}&isDefault=true|false
 */
export const setDefaultDataSource = async (
  organizationId: number,
  dataSourceId: number,
  projectId: number,
  isDefault: boolean = true
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/datasources/${dataSourceId}/default`,
      null,
      { params: { projectId, isDefault } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error setting default data source ${dataSourceId}:`, error);
    throw error;
  }
};
