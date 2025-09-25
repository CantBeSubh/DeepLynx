import nextra from 'nextra'

// Set up Nextra with its configuration
const withNextra = nextra({
  // Add Nextra-specific options here
  search: true,
  search: { codeblocks: true },
})

// Export the final Next.js config with Nextra included
export default withNextra({
  // Add regular Next.js options here
  reactStrictMode: true,
  assetPrefix: process.env.NEXT_PUBLIC_BASE_PATH || '',
  basePath: process.env.NEXT_PUBLIC_BASE_PATH || '',
  webpack: (config, { isServer }) => {
    if (!isServer) {
      config.output.publicPath = `${process.env.NEXT_PUBLIC_BASE_PATH || ''}/_next/`;
    }
    return config;
  },
})