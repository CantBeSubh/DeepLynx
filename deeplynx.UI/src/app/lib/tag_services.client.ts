import { Tag } from '../(home)/types/types';
import { TagResponseDto } from '../(home)/types/responseDTOs';
import api from './api';

export const getTag = async (projectId: number, tagId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/tags/GetTag/${tagId}`);
        return res.data;
    } catch (error) {
        console.error("Error getting a tag:", error);
        throw error;
    }
}

export const getAllTags = async (projectId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/tags/GetAllTags`);
        return res.data;
    } catch (error) {
        console.error("Error getting a tag:", error);
        throw error;
    }
}

export async function createTag(projectId: number, obj: {
    name: string;
}) {
    try {
        const res = await api.post(`/projects/${projectId}/tags/CreateTag`, obj, {
            headers: { "Content-Type": "application/json" },
        });
        return res.data
    } catch (error) {
        console.error("Error getting a tag:", error);
        throw error;
    }
}

export const updateTag = async (
  projectId: number,
  tagId: number,
  obj: { name: string }
): Promise<TagResponseDto> => {
  try {
    const res = await api.put(
      `/projects/${projectId}/tags/UpdateTag/${tagId}`,
      obj,
      {
        headers: { "Content-Type": "application/json" },
      }
    );
    return res.data;
  } catch (error) {
    console.error("Error updating tag:", error);
    throw error;
  }
};