// auth.ts (root)
import NextAuth from "next-auth";
import Okta from "next-auth/providers/okta";


export const runtime = "nodejs";


export const { handlers, auth, signIn, signOut } = NextAuth({
    providers: [
        Okta({
            clientId: process.env.AUTH_OKTA_ID!,
            clientSecret: process.env.AUTH_OKTA_SECRET!,
            issuer: process.env.AUTH_OKTA_ISSUER,
            authorization: {
                params: {
                    scope: "openid profile email email", // Added email scope
                    redirect_uri: process.env.NEXT_PUBLIC_REDIRECT_LINK
                }
            },
        })
    ],
    callbacks: {
        async jwt({ token, account, profile, user }) {

            
            if (account) {
                // Store tokens
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
    secret: process.env.AUTH_SECRET,
});