import api from './api';

//List Permissions
export const getAllPermissions = async (
    labelId?: number,
    projectId?: number,
    organizationId?: number,
    hideArchived: boolean = true,
) => {
    try {
        const res = await api.get(`/permissions/GetAllPermissions`, {
            params: {
                labelId,
                projectId,
                organizationId,
                hideArchived
            }
        });
        return res.data;
    } catch (error) {
        console.error("Error getting all permissions:", error);
        throw error;
    }
}

//Fetch Permission by ID
export const fetchPermissionId = async (
    permissionId: number,
    hideArchived: boolean = true,
) => {
    try {
        const res = await api.get(`/permissions/GetAllPermission/${permissionId}`, {
            params: {
                permissionId,
                hideArchived
            }
        });
        return res.data;
    } catch (error) {
        console.error("Error fetching permission by Id:", error);
        throw error;
    }
}


//Create a Permission
export const createPermission = async (
    name: string,
    action: string,
    labelId?: number,
    description?: string | null,
    projectId?: number,
    organizationId?: number,
) => {
    try {
        const res = await api.post(`/permissions/CreatePermission`, {
            params: {
                name,
                action,
                labelId,
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

//Update a Permission
export async function updatePermission(
  permissionId: number,
  updateData: {
    name?: string | null;
    description?: string | null;
    action?: string | null,
    labelId?: number,
  }
) {
  try {
    const res = await api.put(
      `/permissions/UpdatePermission/${permissionId}`,
      updateData,
      { headers: { "Content-Type": "application/json" } }
    );
    return res.data;
  } catch (error) {
    console.error("Error updating record:", error);
    throw error;
  }
}

//Delete a Permission
export async function deletePermission(permissionId: number) {
  try {
    const res = await api.delete(`/permissions/DeletePermission/${permissionId}`);
    return res.data;
  } catch (error) {
    console.error("API call failed to delete permission:", error);
    throw error;
  }
}

//Archive a Permission

//Unarchive a Permission