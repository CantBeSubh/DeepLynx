// auth.ts (root)
import NextAuth from "next-auth";
import Okta from "next-auth/providers/okta"

console.log("Client ID:", process.env.AUTH_OKTA_ID);
console.log("Client Secret exists:", process.env.AUTH_OKTA_SECRET);
console.log("Issuer:", process.env.AUTH_OKTA_ISSUER);

export const {
    handlers,
    auth,
    signIn,
    signOut,
} = NextAuth({
    providers: [
        Okta({
            clientId: process.env.AUTH_OKTA_ID!,
            clientSecret: process.env.AUTH_OKTA_SECRET!,
            issuer: process.env.AUTH_OKTA_ISSUER,
            authorization: {
                params: {
                    scope: "openid profile email", // Added email scope
                    redirect_uri: "http://localhost:3000/api/auth/callback/okta",
                },
            },
        })
    ],
    callbacks: {
        async jwt({ token, account, profile, user }) {
            console.log("JWT CALLBACK TRIGGERED");
            console.log("Account exists:", !!account);
            console.log("Profile exists:", !!profile);
            console.log("User exists:", !!user);
            
            if (account) {
                console.log("Account data:", account);
            }
            if (profile) {
                console.log("Profile data:", profile);
                token.oktaId = profile.sub;
                token.username = (profile as any).preferred_username;
                token.groups = (profile as any).groups || [];
            }
            return token;
        },
        
        async session({ session, token }) {
            console.log("SESSION CALLBACK TRIGGERED");
            console.log("Token:", token);
            console.log("Session before:", session);
            
            if (token) {
                (session.user as any).oktaId = token.oktaId;
                (session.user as any).username = token.username;
                (session.user as any).groups = token.groups;
            }
            
            console.log("Session after:", session);
            return session;
        },
    },
    secret: process.env.AUTH_SECRET,
});