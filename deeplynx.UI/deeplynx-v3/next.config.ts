import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  env: {
    NEXT_PUBLIC_OKTA_CLIENT_ID: process.env.NEXT_PUBLIC_OKTA_CLIENT_ID,
    NEXT_PUBLIC_OKTA_CLIENT_SECRET: process.env.NEXT_PUBLIC_OKTA_CLIENT_SECRET,
    NEXT_PUBLIC_OKTA_ISSUER: process.env.NEXT_PUBLIC_OKTA_ISSUER,
    NEXT_PUBLIC_REDIRECT_LINK: process.env.NEXT_PUBLIC_REDIRECT_LINK,
    NEXT_PUBLIC_AUTH_SECRET: process.env.NEXT_PUBLIC_AUTH_SECRET,
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
  },
  /* other config options here */
};

export default nextConfig;
