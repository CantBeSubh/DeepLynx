// lib/api.ts
import axios from 'axios';
import { getSession } from 'next-auth/react';

const api = axios.create({
  baseURL: process.env.BACKEND_BASE_URL,
});

// Request interceptor to add token
api.interceptors.request.use(async (config) => {
  const session = await getSession();
  if (session?.tokens?.access_token) {
    config.headers.Authorization = `Bearer ${session.tokens.access_token}`;
  }
  return config;
});

export default api;