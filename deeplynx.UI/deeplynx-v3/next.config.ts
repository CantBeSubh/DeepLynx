// next.config.js
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  serverRuntimeConfig: {
    // Server-side only variables (these don't need NEXT_PUBLIC_)
    NEXT_PUBLIC_OKTA_CLIENT_SECRET: process.env.NEXT_PUBLIC_OKTA_CLIENT_SECRET,
    NEXT_PUBLIC_AUTH_SECRET: process.env.NEXT_PUBLIC_AUTH_SECRET,
  },
  publicRuntimeConfig: {
    // Client-side runtime variables
    NEXT_PUBLIC_OKTA_CLIENT_ID: process.env.NEXT_PUBLIC_OKTA_CLIENT_ID,
    NEXT_PUBLIC_OKTA_ISSUER: process.env.NEXT_PUBLIC_OKTA_ISSUER,
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
    NEXT_PUBLIC_REDIRECT_LINK: process.env.NEXT_PUBLIC_REDIRECT_LINK,
  },
};

export default nextConfig;