import NextAuth from "next-auth";
import OktaProvider from "next-auth/providers/okta"

export const {
    handlers,
    auth,
    signIn,
    signOut,
} = NextAuth({
    providers: [
        OktaProvider({
            clientId: process.env.NEXT_PUBLIC_OKTA_CLIENT_ID!,
            clientSecret: process.env.NEXT_PUBLIC_OKTA_CLIENT_SECRET!,
            issuer: process.env.NEXT_PUBLIC_OKTA_ISSUER,
            authorization: {
                params: {
                    scope: "openid profile",
                    redirect_uri: process.env.NEXT_PUBLIC_REDIRECT_LINK,
                },
            },
        })
    ],
});
