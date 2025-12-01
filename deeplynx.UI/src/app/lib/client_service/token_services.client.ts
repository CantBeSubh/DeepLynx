import api from "./api";

export async function getUserApiKeys() {
  try {
    const res = await api.get("/oauth/keys");

    return res.data;
  } catch (err) {
    console.error("Error fetching API keys:", err);
    throw err;
  }
}
