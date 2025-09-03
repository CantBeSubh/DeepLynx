import NextAuth from "next-auth";
import Okta from "next-auth/providers/okta";


export const runtime = "nodejs";


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
        })
    ],
    logger: {
        warn(code) { console.warn("NextAuth warn", code); },
        debug(code, metadata) { console.debug("NextAuth debug", code, metadata); },
    },
});