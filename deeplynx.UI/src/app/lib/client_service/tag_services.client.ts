import api from './client_service/api';
import { TagResponseDto } from '../(home)/types/responseDTOs';
import { CreateTagRequestDto, UpdateTagRequestDto } from '../(home)/types/requestDTOs';

/**
 * Get all tags for a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param hideArchived - Flag to hide archived tags (default: true)
 * @returns Promise with array of TagResponseDto
 */
export const getAllTags = async (
  organizationId: number,
  projectId: number,
  hideArchived: boolean = true
): Promise<TagResponseDto[]> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/tags`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error("Error getting all tags:", error);
    throw error;
  }
};

/**
 * Get a specific tag by ID
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param tagId - The ID of the tag
 * @param hideArchived - Flag to hide archived tags (default: true)
 * @returns Promise with TagResponseDto
 */
export const getTag = async (
  organizationId: number,
  projectId: number,
  tagId: number,
  hideArchived: boolean = true
): Promise<TagResponseDto> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/tags/${tagId}`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting tag ${tagId}:`, error);
    throw error;
  }
};

/**
 * Get all tags from multiple projects
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project (required for route)
 * @param projectIds - Array of project IDs to retrieve tags from
 * @param hideArchived - Flag to hide archived tags (default: true)
 * @returns Promise with array of TagResponseDto
 */
export const getAllTagsMultiProject = async (
  organizationId: number,
  projectId: number,
  projectIds: number[],
  hideArchived: boolean = true
): Promise<TagResponseDto[]> => {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/tags/multiproject`,
      { params: { projectIds, hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error("Error getting tags from multiple projects:", error);
    throw error;
  }
};

/**
 * Create a new tag
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param dto - The tag creation request DTO
 * @returns Promise with TagResponseDto
 */
export async function createTag(
  organizationId: number,
  projectId: number,
  dto: CreateTagRequestDto
): Promise<TagResponseDto> {
  try {
    const res = await api.post(
      `/organizations/${organizationId}/projects/${projectId}/tags`,
      dto,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error("Error creating tag:", error);
    throw error;
  }
}

/**
 * Bulk create tags
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param tags - Array of tag creation request DTOs
 * @returns Promise with array of TagResponseDto
 */
export async function bulkCreateTags(
  organizationId: number,
  projectId: number,
  tags: CreateTagRequestDto[]
): Promise<TagResponseDto[]> {
  try {
    const res = await api.post(
      `/organizations/${organizationId}/projects/${projectId}/tags/bulk`,
      tags,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error("Error bulk creating tags:", error);
    throw error;
  }
}

/**
 * Update a tag
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param tagId - The ID of the tag to update
 * @param dto - The tag update request DTO
 * @returns Promise with TagResponseDto
 */
export const updateTag = async (
  organizationId: number,
  projectId: number,
  tagId: number,
  dto: UpdateTagRequestDto
): Promise<TagResponseDto> => {
  try {
    const res = await api.put(
      `/organizations/${organizationId}/projects/${projectId}/tags/${tagId}`,
      dto,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error updating tag ${tagId}:`, error);
    throw error;
  }
};

/**
 * Delete a tag
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param tagId - The ID of the tag to delete
 * @returns Promise with success message
 */
export const deleteTag = async (
  organizationId: number,
  projectId: number,
  tagId: number
): Promise<{ message: string }> => {
  try {
    const res = await api.delete(
      `/organizations/${organizationId}/projects/${projectId}/tags/${tagId}`
    );
    return res.data;
  } catch (error) {
    console.error(`Error deleting tag ${tagId}:`, error);
    throw error;
  }
};

/**
 * Archive or unarchive a tag
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param tagId - The ID of the tag to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export const archiveTag = async (
  organizationId: number,
  projectId: number,
  tagId: number,
  archive: boolean
): Promise<{ message: string }> => {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/projects/${projectId}/tags/${tagId}`,
      null,
      { params: { archive } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error ${archive ? 'archiving' : 'unarchiving'} tag ${tagId}:`, error);
    throw error;
  }
};