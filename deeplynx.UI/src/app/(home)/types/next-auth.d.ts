// types/next-auth.d.ts
import NextAuth from "next-auth"

declare module "next-auth" {
  interface Session {
    tokens?: {
      access_token?: string
      id_token?: string
      expires_at?: number
    }
  }
}