"use client";

import api from "./api"; // Import your API utility
export interface Group {
  id: number | null;
  name: string;
  description?: string | null;
}

/** ===== Client calls (browser; cookie/session-based) ===== */

export async function getAllGroups() {
  try {
    const res = await api.get("/groups");
    return res.data;
  } catch (error) {
    console.error("Failed to fetch groups:", error);
    throw new Error("Could not retrieve groups. Please try again later.");
  }
}

export async function getGroupById(groupId: string) {
  try {
    const res = await api.get(`/groups/${groupId}`);
    return res.data;
  } catch (error) {
    console.error(`Failed to fetch group with ID ${groupId}:`, error);
    throw new Error("Could not retrieve the group. Please try again later.");
  }
}

export async function createGroup(data: { name: string; description?: string | null }) {
  try {
    const res = await api.post("/groups/CreateGroup", data, {
      headers: { "Content-Type": "application/json" },
    });
    return res.data;
  } catch (error) {
    console.error("Failed to create group:", error);
    throw new Error("Could not create the group. Please ensure all fields are correct.");
  }
}

export async function updateGroup(groupId: string, data: { name?: string; description?: string }) {
  try {
    const res = await api.put(`/groups/${groupId}`, data, {
      headers: { "Content-Type": "application/json" },
    });
    return res.data;
  } catch (error) {
    console.error(`Failed to update group with ID ${groupId}:`, error);
    throw new Error("Could not update the group. Please try again later.");
  }
}

export async function deleteGroup(groupId: string) {
  try {
    await api.delete(`/groups/${groupId}`);
  } catch (error) {
    console.error(`Failed to delete group with ID ${groupId}:`, error);
    throw new Error("Could not delete the group. Please try again later.");
  }
}

export async function archiveGroup(groupId: string) {
  try {
    const res = await api.post(`/groups/${groupId}/archive`);
    return res.data;
  } catch (error) {
    console.error(`Failed to archive group with ID ${groupId}:`, error);
    throw new Error("Could not archive the group. Please try again later.");
  }
}

export async function unarchiveGroup(groupId: string) {
  try {
    const res = await api.post(`/groups/${groupId}/unarchive`);
    return res.data;
  } catch (error) {
    console.error(`Failed to unarchive group with ID ${groupId}:`, error);
    throw new Error("Could not unarchive the group. Please try again later.");
  }
}

export async function addUserToGroup(groupId: string, userId: string) {
  try {
    const res = await api.post(`/groups/${groupId}/addUser`, { userId });
    return res.data;
  } catch (error) {
    console.error(`Failed to add user ${userId} to group with ID ${groupId}:`, error);
    throw new Error("Could not add the user to the group. Please try again later.");
  }
}

export async function removeUserFromGroup(groupId: string, userId: string) {
  try {
    const res = await api.post(`/groups/${groupId}/removeUser`, { userId });
    return res.data;
  } catch (error) {
    console.error(`Failed to remove user ${userId} from group with ID ${groupId}:`, error);
    throw new Error("Could not remove the user from the group. Please try again later.");
  }
}