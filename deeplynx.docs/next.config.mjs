import nextra from 'nextra'

const withNextra = nextra({
  search: { codeblocks: true }
})

const basePath = process.env.NEXT_PUBLIC_DOCS_PATH || '';

export default withNextra({
// Add regular Next.js options here
  reactStrictMode: true,
  trailingSlash: true, 
  basePath
})
