// auth.ts (root)
import NextAuth from "next-auth";
import { JWT, JWTEncodeParams, JWTDecodeParams } from "next-auth/jwt";
import Okta from "next-auth/providers/okta";
import jsonWebToken from "jsonwebtoken";

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
                    redirect_uri: process.env.NEXT_PUBLIC_REDIRECT_LINK!
                }
            },
        })
    ],
    callbacks: {
        async jwt({ token, account, profile }) {
            // Store Okta tokens and user info in JWT
            if (account) {
                token.access_token = account.access_token;
                token.id_token = account.id_token;
                token.expires_at = account.expires_at;
            }

            if (profile) {
                token.oktaId = profile.sub;
                token.username = (profile as any).preferred_username;
                token.groups = (profile as any).groups || [];
            }

            return token;
        },

        async session({ session, token }) {
            // Add Okta-specific data to session
            if (token) {
                (session.user as any).oktaId = token.oktaId;
                (session.user as any).username = token.username;
                (session.user as any).groups = token.groups;

                (session as any).tokens = {
                    access_token: token.access_token,
                    id_token: token.id_token,
                    expires_at: token.expires_at,
                };
            }

            return session;
        },
    },
    pages: {
        signIn: '/login/signin',
    },
    session: {
        strategy: "jwt",
        maxAge: 30 * 24 * 60 * 60, // 30 days
    },
    jwt: {
        encode: async (params: JWTEncodeParams) => {
            const { secret, token } = params;
            if (!token) {
                throw new Error("Token is undefined");
            }
            // Ensure secret is treated as a string
            const signingSecret = typeof secret === 'string' ? secret : secret[0];
            // Use the 'jsonwebtoken' library to sign the token
            return jsonWebToken.sign(token, signingSecret);
        },
        decode: async (params: JWTDecodeParams) => {
            const { secret, token } = params;
            if (!token) {
                throw new Error("Token is undefined");
            }
            // Ensure secret is treated as a string
            const verifyingSecret = typeof secret === 'string' ? secret : secret[0];
            // Use the 'jsonwebtoken' library to verify and decode the token
            return jsonWebToken.verify(token, verifyingSecret) as JWT;
        },
    },
    secret: process.env.NEXTAUTH_SECRET!, // Ensure TypeScript knows this is a string
});
