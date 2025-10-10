import nextra from 'nextra'

const withNextra = nextra({
  search: { codeblocks: true }
})

export default withNextra({
// Add regular Next.js options here
  basePath: '/docs',
  reactStrictMode: true,
})
