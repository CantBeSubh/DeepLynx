import nextra from 'nextra'

const withNextra = nextra({
  search: { codeblocks: true }
})

export default withNextra({
// Add regular Next.js options here
  basePath:  process.env.NEXT_PUBLIC_DOCS_PATH ? `${process.env.NEXT_PUBLIC_DOCS_PATH}` : "",
  reactStrictMode: true,
})
