import nextra from 'nextra'

const withNextra = nextra({
  search: { codeblocks: true }
})

//TODO: switch to env var
//const basePath = process.env.DOCS_PATH || '';

export default withNextra({
// Add regular Next.js options here
  basePath: '/docs',
  reactStrictMode: true,
})
