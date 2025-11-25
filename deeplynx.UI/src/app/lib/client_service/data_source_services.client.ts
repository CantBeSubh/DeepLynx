'use client';

import { CreateDataSourceRequestDto, UpdateDataSourceRequestDto } from "@/app/(home)/types/requestDTOs";
import { DataSourceResponseDto } from "@/app/(home)/types/responseDTOs";
import api from "./api";


/**
 * Get all data sources for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param hideArchived - Flag to hide archived data sources (default: true)
 * @returns Promise with array of DataSourceResponseDto
 */
export const getAllDataSources = async (
  organizationId: number,
  projectId: number,
  hideArchived: boolean = true
): Promise<DataSourceResponseDto[]> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/datasources`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error("Error getting all data sources:", error);
    throw error;
  }
};

/**
 * Get all data sources from multiple projects
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project (required for route)
 * @param projectIds - Array of project IDs to retrieve data sources from
 * @param hideArchived - Flag to hide archived data sources (default: true)
 * @returns Promise with array of DataSourceResponseDto
 */
export const getAllDataSourcesMultiProject = async (
  organizationId: number,
  projectId: number,
  projectIds: number[],
  hideArchived: boolean = true
): Promise<DataSourceResponseDto[]> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/datasources/multiproject`,
      { params: { projectIds, hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error("Error getting data sources from multiple projects:", error);
    throw error;
  }
};

/**
 * Get a specific data source
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dataSourceId - The ID of the data source
 * @param hideArchived - Flag to hide archived data sources (default: true)
 * @returns Promise with DataSourceResponseDto
 */
export const getDataSource = async (
  organizationId: number,
  projectId: number,
  dataSourceId: number,
  hideArchived: boolean = true
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/datasources/${dataSourceId}`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Get the default data source for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @returns Promise with DataSourceResponseDto
 */
export const getDefaultDataSource = async (
  organizationId: number,
  projectId: number
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/datasources/default`
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting default data source for project ${projectId}:`, error);
    throw error;
  }
};

/**
 * Create a new data source
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dto - The data source creation request DTO
 * @returns Promise with DataSourceResponseDto
 */
export const createDataSource = async (
  organizationId: number,
  projectId: number,
  dto: CreateDataSourceRequestDto
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.post(
      `/organizations/${organizationId}/projects/${projectId}/datasources`,
      dto
    );
    return res.data;
  } catch (error) {
    console.error("Error creating data source:", error);
    throw error;
  }
};

/**
 * Update a data source
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dataSourceId - The ID of the data source to update
 * @param dto - The data source update request DTO
 * @returns Promise with DataSourceResponseDto
 */
export const updateDataSource = async (
  organizationId: number,
  projectId: number,
  dataSourceId: number,
  dto: UpdateDataSourceRequestDto
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.put(
      `/organizations/${organizationId}/projects/${projectId}/datasources/${dataSourceId}`,
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
 * @param projectId - The ID of the project
 * @param dataSourceId - The ID of the data source to delete
 * @returns Promise with success message
 */
export const deleteDataSource = async (
  organizationId: number,
  projectId: number,
  dataSourceId: number
): Promise<{ message: string }> => {
  try {
    const res = await api.delete(
      `/organizations/${organizationId}/projects/${projectId}/datasources/${dataSourceId}`
    );
    return res.data;
  } catch (error) {
    console.error(`Error deleting data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Archive or unarchive a data source
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dataSourceId - The ID of the data source to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export const archiveDataSource = async (
  organizationId: number,
  projectId: number,
  dataSourceId: number,
  archive: boolean
): Promise<{ message: string }> => {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/projects/${projectId}/datasources/${dataSourceId}`,
      null,
      { params: { archive } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error ${archive ? 'archiving' : 'unarchiving'} data source ${dataSourceId}:`, error);
    throw error;
  }
};

/**
 * Set a data source as the default for the project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dataSourceId - The ID of the data source to set as default
 * @param isDefault - True to set as default (default: true)
 * @returns Promise with DataSourceResponseDto
 */
export const setDefaultDataSource = async (
  organizationId: number,
  projectId: number,
  dataSourceId: number,
  isDefault: boolean = true
): Promise<DataSourceResponseDto> => {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/projects/${projectId}/datasources/${dataSourceId}/default`,
      null,
      { params: { isDefault } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error setting default data source ${dataSourceId}:`, error);
    throw error;
  }
};