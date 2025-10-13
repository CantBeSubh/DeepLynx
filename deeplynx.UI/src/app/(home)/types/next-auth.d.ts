// src/app/(home)/types/next-auth.d.ts
import { DefaultSession, DefaultJWT } from "next-auth";
import { JWT } from "next-auth/jwt";

declare module "next-auth" {
    interface Session {
        user: {
            oktaId?: string;
            username?: string;
            groups?: string[];
        } & DefaultSession["user"];
        tokens?: {
            access_token?: string;
            id_token?: string;
            expires_at?: number;
        };
        error?: string;
    }
}

declare module "next-auth/jwt" {
    interface JWT {
        access_token?: string;
        id_token?: string;
        expires_at?: number;
        refresh_token?: string;
        oktaId?: string | null;
        username?: string | null;
        groups?: string[];
        error?: string;
        user?: {
            name?: string | null;
            email?: string | null;
        };
    }
}