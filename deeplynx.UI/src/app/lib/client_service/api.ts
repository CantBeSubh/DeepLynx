// lib/client_service/api.ts

import axios from 'axios';
import { getSession } from 'next-auth/react';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL
    ? `${process.env.NEXT_PUBLIC_API_URL}`
    : "/api/v1",
});

// Request interceptor to add token
api.interceptors.request.use(async (config) => {
  // Skip auth header if frontend authentication is disabled
  const isAuthDisabled =
    process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION === "true";

  if (isAuthDisabled) {
    // Don't add authorization header when auth is disabled
    return config;
  }

  // Only get session when auth is enabled
  const session = await getSession();
  if (session?.tokens?.access_token) {
    config.headers.Authorization = `Bearer ${session.tokens.access_token}`;
  }

  return config;
});

export default api;