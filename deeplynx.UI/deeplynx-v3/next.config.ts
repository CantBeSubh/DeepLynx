import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  publicRuntimeConfig: {
    // These will be populated at runtime from Kubernetes
    OKTA_ISSUER: process.env.NEXT_PUBLIC_OKTA_ISSUER,
    OKTA_CLIENT_ID: process.env.NEXT_PUBLIC_OKTA_CLIENT_ID,
    API_URL: process.env.NEXT_PUBLIC_API_URL,
    REDIRECT_LINK: process.env.NEXT_PUBLIC_REDIRECT_LINK,
  },
  serverRuntimeConfig: {
    // Server-only secrets
    OKTA_CLIENT_SECRET: process.env.NEXT_PUBLIC_OKTA_CLIENT_SECRET,
    AUTH_SECRET: process.env.NEXT_PUBLIC_AUTH_SECRET,
  },
};

export default nextConfig;