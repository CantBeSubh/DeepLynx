import NextAuth from "next-auth";
import Okta from "next-auth/providers/okta";
import Credentials from "next-auth/providers/credentials";


export const runtime = "nodejs";

// Fail early if envs are missing (prints exact key in server log)
["OKTA_CLIENT_ID", "OKTA_CLIENT_SECRET", "OKTA_ISSUER", "NEXTAUTH_URL", "NEXTAUTH_SECRET"].forEach(k => {
    if (!process.env[k]) throw new Error(`Missing env ${k}`);
});

export const { handlers, auth, signIn, signOut } = NextAuth({
    providers: [
        Okta({
            clientId: process.env.OKTA_CLIENT_ID!,
            clientSecret: process.env.OKTA_CLIENT_SECRET!,
            issuer: process.env.OKTA_ISSUER,
            authorization: {
                params: {
                    scope: "openid profile email",
                    redirect_uri: process.env.NEXT_PUBLIC_REDIRECT_LINK
                }
            },
        }),
        Credentials({
            name: "Test",
            credentials: { username: { label: "Username" }, password: { label: "Password", type: "password" } },
            async authorize() { return { id: "1", name: "Dev User" }; },
        }),
    ],
    logger: {
        warn(code) { console.warn("NextAuth warn", code); },
        debug(code, metadata) { console.debug("NextAuth debug", code, metadata); },
    },
});