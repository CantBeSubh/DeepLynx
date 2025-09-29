//src/middleware.ts

import { NextResponse } from "next/server";
import { auth } from "../auth";

export default auth((req) => {
  const pathname = req.nextUrl.pathname;
  const isAuth = !!req.auth;
  
  // Public routes that don't need auth
  const isPublicRoute = 
    pathname.startsWith("/login") ||
    pathname.startsWith("/api/auth") ||
    pathname.startsWith("/assets");
  
  // Protect routes
  if (!isPublicRoute && !isAuth) {
    const url = new URL("/login/signin", req.url);
    url.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(url);
  }
  
  return NextResponse.next();
});

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};