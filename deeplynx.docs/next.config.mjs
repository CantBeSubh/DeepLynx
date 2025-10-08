import nextra from 'nextra'

const withNextra = nextra({
  search: { codeblocks: true }
})

export default withNextra({
// Add regular Next.js options here
  reactStrictMode: true,
  assetPrefix: process.env.NEXT_PUBLIC_DOCS_PATH || '',
  basePath: process.env.NEXT_PUBLIC_DOCS_PATH || '',
  trailingSlash: true, 
  webpack: (config, { isServer }) => {
    if (!isServer) {
      config.output.publicPath = `${process.env.NEXT_PUBLIC_DOCS_PATH || ''}/_next/`;
    }
    return config;
  },
})
