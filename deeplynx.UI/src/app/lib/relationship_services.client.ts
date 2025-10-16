// src/app/lib/relationship_services.client.ts

import api from "./api";

export const getAllRelationships = async (projectId: number | string) => {
  const { data } = await api.get(`/projects/${projectId}/relationships/GetAllRelationships`);
  return data;
};