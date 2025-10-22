import api from './api';

export async function createApiKey(
) {
  try {
    const res = await api.post(
      `/token/CreateApiKey`,
      {
        headers: { "Content-Type": "application/json" }
      }
    );

    return res.data;
  } catch (error) {
    console.error("Error creating api key: ", error);
    throw error;
  }
}
export async function deleteApiKey(key:string ) {
  try {
    console.log("Key to delete: " +key)
    const res = await api.delete(
      `/token/DeleteApiKey/${key}`,
    {
      headers: { "Content-Type": "application/json" },
    }
    );

    return res.data;
  } catch (error) {
    console.error("Error deleting api key: ", error);
    throw error;
  }
}

export async function getAllKeysByUser() {
  try{
    const res = await api.get(
      `/token/GetAllUserKeys`,
      {
        headers: { "Content-Type": "application/json" }
      }
    );
    console.log(res.data)
    return res.data;
  } catch (error) {
    console.error("Error getting api keys for user: ", error);
    throw error;
  }
}

