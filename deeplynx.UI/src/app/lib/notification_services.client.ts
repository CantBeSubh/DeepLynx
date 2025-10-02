"use client";

import api from "./api";

/** ---- Browser calls (with session cookies) ---- */

export async function sendEmail(email: string, name: string = "User") {
  try {
    const res = await api.post(`/notification/SendEmail`, null, {
      params: { email, name },
    });
    return res.data;
  } catch (error) {
    console.error("API call failed:", error);
    throw error;
  }
}