// src/app/lib/group_services.client.ts
"use client";

import api from "./api";
import { GroupResponseDto } from "../(home)/types/responseDTOs";

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function getAllGroups(
  organizationId?: number | string,
  hideArchived: boolean = true
) {
  const params: Record<string, string | number | boolean> = { hideArchived };
  
  if (organizationId !== undefined) {
    params.organizationId = organizationId;
  }

  const res = await api.get("/groups/GetAllGroups", { params });
  return res.data;
}

export async function getGroup(groupId: number | string) {
  const res = await api.get(`/groups/GetGroup/${groupId}`);
  return res.data;
}

export async function createGroup(
  organizationId: number | string,
  name: string,
  description: string
): Promise<GroupResponseDto> {
  const res = await api.post("/groups/CreateGroup", {
    organizationId,
    name,
    description,
  }, {
    headers: { "Content-Type": "application/json" },
  });
  return res.data;
}

export async function updateGroup(
  groupId: number | string,
  name: string,
  description: string
): Promise<GroupResponseDto> {
  const res = await api.put(`/groups/UpdateGroup/${groupId}`, {
    name,
    description,
  }, {
    headers: { "Content-Type": "application/json" },
  });
  return res.data;
}

export async function deleteGroup(groupId: number | string): Promise<void> {
  await api.delete(`/groups/DeleteGroup/${groupId}`);
}

export async function getGroupMembers(groupId: number | string) {
  const res = await api.get(`/groups/GetGroupMembers/${groupId}`);
  return res.data;
}

export async function addMemberToGroup(
  groupId: number | string,
  userId: number | string
): Promise<void> {
  const res = await api.post(
    `/groups/AddMemberToGroup?groupId=${groupId}&userId=${userId}`,
    { groupId, userId },
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return res.data;
}

export async function removeMemberFromGroup(
  groupId: number | string,
  userId: number | string
): Promise<void> {
  await api.delete(
    `/groups/RemoveMemberFromGroup?groupId=${groupId}&userId=${userId}`,
    {
      headers: { "Content-Type": "application/json" },
    }
  );
}