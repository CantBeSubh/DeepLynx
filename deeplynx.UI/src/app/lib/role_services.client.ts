import api from './api';

//Get All Roles
export const getAllRoles = async (
    projectId?: number,
    organizationId?: number,
    hideArchived: boolean = true
) => {
    try {
        const res = await api.get(`/roles/GetAllRoles`, {
            params: {
                projectId,
                organizationId,
                hideArchived
            }
        });
        return res.data;
    } catch (error) {
        console.error("Error getting all roles:", error);
        throw error;
    }
}

//Fetch Role by ID
export const fetchRoleId = async (
    roleId: number,
    hideArchived: boolean = true
) => {
    try {
        const res = await api.get(`/roles/GetRole/${roleId}`, {
            params: {
                roleId,
                hideArchived
            }
        });
        return res.data;
    } catch (error) {
        console.error("Error fetching role by Id:", error);
        throw error;
    }
}

//Create a Role
export const createRole = async (
    name: string,
    description?: string | null,
    projectId?: number,
    organizationId?: number,
) => {
    try {
        const res = await api.post(`/roles/CreateRole`, {
            params: {
                name,
                description,
                projectId,
                organizationId,
            }
        });
        return res.data;
    } catch (error) {
        console.error("Error fetching role by Id:", error);
        throw error;
    }
}

//Update a Role
export async function updateRole(
  roleId: number,
  updateData: {
    name?: string | null;
    description?: string | null;
  }
) {
  try {
    const res = await api.put(
      `/roles/UpdateRole/${roleId}`,
      updateData,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error("Error updating record:", error);
    throw error;
  }
}

//Delete a Role
export async function deleteRole(roleId: number) {
  try {
    const res = await api.delete(`/roles/DeleteRole/${roleId}`);
    return res.data;
  } catch (error) {
    console.error("API call failed to delete role:", error);
    throw error;
  }
}

//Archive a Role

//Unarchive a Role

//List Permissions by Role
export async function listRolePermissions(roleId: number) {
  try {
    const res = await api.get(`/roles/GetPermissionsByRole/${roleId}`);
    return res.data;
  } catch (error) {
    console.error("API call failed to get permissions by role:", error);
    throw error;
  }
}

//Add Permission to Role


//Remove Permission from Role


//Set Permissions for a Role
export async function setRolePermissions(roleId: number) {
  try {
    const res = await api.get(`/roles/SetPermissionsByRole/${roleId}`);
    return res.data;
  } catch (error) {
    console.error("API call failed to set permissions to thr role:", error);
    throw error;
  }
}