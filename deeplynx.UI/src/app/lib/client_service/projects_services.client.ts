// src/app/lib/projects_services.client.ts
"use client";

import api from "./client_service/api";
import { ProjectMemberResponseDto, ProjectResponseDto, ProjectStatResponseDto } from "../(home)/types/responseDTOs";
import { CreateProjectRequestDto, UpdateProjectRequestDto } from "../(home)/types/requestDTOs";
/**
 * Get all projects for an organization
 * @param organizationId - The ID of the organization
 * @param hideArchived - Flag to hide archived projects (default: true)
 * @returns Promise with array of ProjectResponseDto
 */
export async function getAllProjects(
  organizationId: number,
  hideArchived: boolean = true
): Promise<ProjectResponseDto[]> {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error("Error getting all projects:", error);
    throw error;
  }
}

/**
 * Get a specific project by ID
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param hideArchived - Flag to hide archived projects (default: true)
 * @returns Promise with ProjectResponseDto
 */
export async function getProject(
  organizationId: number,
  projectId: number,
  hideArchived: boolean = true
): Promise<ProjectResponseDto> {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}`,
      { params: { hideArchived } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Create a new project
 * @param organizationId - The ID of the organization
 * @param dto - The project creation request DTO
 * @returns Promise with ProjectResponseDto
 */
export async function createProject(
  organizationId: number,
  dto: CreateProjectRequestDto
): Promise<ProjectResponseDto> {
  try {
    const res = await api.post(
      `/organizations/${organizationId}/projects`,
      dto,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error("Error creating project:", error);
    throw error;
  }
}

/**
 * Update a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project to update
 * @param dto - The project update request DTO
 * @returns Promise with ProjectResponseDto
 */
export async function updateProject(
  organizationId: number,
  projectId: number,
  dto: UpdateProjectRequestDto
): Promise<ProjectResponseDto> {
  try {
    const res = await api.put(
      `/organizations/${organizationId}/projects/${projectId}`,
      dto,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error updating project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Delete a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project to delete
 * @returns Promise with success message
 */
export async function deleteProject(
  organizationId: number,
  projectId: number
): Promise<{ message: string }> {
  try {
    const res = await api.delete(
      `/organizations/${organizationId}/projects/${projectId}`
    );
    return res.data;
  } catch (error) {
    console.error(`Error deleting project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Archive or unarchive a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project to archive/unarchive
 * @param archive - True to archive, false to unarchive
 * @returns Promise with success message
 */
export async function archiveProject(
  organizationId: number,
  projectId: number,
  archive: boolean
): Promise<{ message: string }> {
  try {
    const res = await api.patch(
      `/organizations/${organizationId}/projects/${projectId}`,
      null,
      { params: { archive } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error ${archive ? 'archiving' : 'unarchiving'} project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Get project statistics
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @returns Promise with ProjectStatResponseDto
 */
export async function getProjectStats(
  organizationId: number,
  projectId: number
): Promise<ProjectStatResponseDto> {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/stats`
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting stats for project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Get all members of a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @returns Promise with project members data
 */
export async function getProjectMembers(
  organizationId: number,
  projectId: number
): Promise<ProjectMemberResponseDto[]> {
  try {
    const res = await api.get(
      `/organizations/${organizationId}/projects/${projectId}/members`
    );
    return res.data;
  } catch (error) {
    console.error(`Error getting members for project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Add a user or group to a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param roleId - Optional role ID
 * @param userId - Optional user ID (required if not providing groupId)
 * @param groupId - Optional group ID (required if not providing userId)
 * @returns Promise with success message
 */
export async function addMemberToProject(
  organizationId: number,
  projectId: number,
  roleId?: number,
  userId?: number,
  groupId?: number
): Promise<{ message: string }> {
  try {
    const res = await api.post(
      `/organizations/${organizationId}/projects/${projectId}/members`,
      null,
      { params: { roleId, userId, groupId } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error adding member to project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Update a member's role in a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param roleId - The new role ID
 * @param userId - Optional user ID (required if not providing groupId)
 * @param groupId - Optional group ID (required if not providing userId)
 * @returns Promise with success message
 */
export async function updateProjectMemberRole(
  organizationId: number,
  projectId: number,
  roleId: number,
  userId?: number,
  groupId?: number
): Promise<{ message: string }> {
  try {
    const res = await api.put(
      `/organizations/${organizationId}/projects/${projectId}/members`,
      null,
      { params: { roleId, userId, groupId } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error updating member role in project ${projectId}:`, error);
    throw error;
  }
}

/**
 * Remove a user or group from a project
 * @param organizationId - The ID of the organization
 * @param projectId - The ID of the project
 * @param userId - Optional user ID (required if not providing groupId)
 * @param groupId - Optional group ID (required if not providing userId)
 * @returns Promise with success message
 */
export async function removeMemberFromProject(
  organizationId: number,
  projectId: number,
  userId?: number,
  groupId?: number
): Promise<{ message: string }> {
  try {
    const res = await api.delete(
      `/organizations/${organizationId}/projects/${projectId}/members`,
      { params: { userId, groupId } }
    );
    return res.data;
  } catch (error) {
    console.error(`Error removing member from project ${projectId}:`, error);
    throw error;
  }
}

// /**
//  * Get records from multiple projects
//  * @param projectIds - Array of project IDs
//  * @param hideArchived - Flag to hide archived records (default: true)
//  * @param opts - Optional configuration like abort signal
//  * @returns Promise with records data
//  */
// export async function getAllRecordsForMultipleProjects(
//   projectIds: number[],
//   hideArchived: boolean = true,
//   opts?: { signal?: AbortSignal }
// ): Promise<any> {
//   try {
//     const query =
//       projectIds.map((id) => `projects=${encodeURIComponent(id)}`).join("&") +
//       `&hideArchived=${hideArchived}`;

//     const res = await api.get(`/projects/MultiProjectRecords?${query}`, {
//       signal: opts?.signal,
//     });
//     return res.data;
//   } catch (error) {
//     console.error("Error getting records for multiple projects:", error);
//     throw error;
//   }
// }