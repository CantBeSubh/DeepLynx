import nextra from 'nextra'

const withNextra = nextra({
  search: true,
  search: { codeblocks: true },
})

export default withNextra({
// Add regular Next.js options here
  reactStrictMode: true,
  assetPrefix: process.env.NEXT_PUBLIC_BASE_PATH || '',
  basePath: process.env.NEXT_PUBLIC_BASE_PATH || '',
  trailingSlash: true, 
  webpack: (config, { isServer }) => {
    if (!isServer) {
      config.output.publicPath = `${process.env.NEXT_PUBLIC_BASE_PATH || ''}/_next/`;
    }
    return config;
  },
})
