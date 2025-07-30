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
            clientId: process.env.OKTA_CLIENT_ID!,
            clientSecret: process.env.OKTA_CLIENT_SECRET!,
            issuer: process.env.OKTA_ISSUER,
            authorization: {
                params: {
                    scope: "openid profile",
                    redirect_uri: process.env.REDIRECT_LINK,
                },
            },
        })
    ],
});
