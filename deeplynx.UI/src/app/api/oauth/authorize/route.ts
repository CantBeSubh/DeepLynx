// src/app/api/oauth/authorize/route.ts
import { auth } from "../../../../../auth";
import { NextRequest, NextResponse } from "next/server";

/**
 * OAuth 2.0 Authorization Endpoint Proxy
 * 
 * This endpoint acts as a proxy between external OAuth clients and the C# backend.
 * It ensures the user is authenticated via NextAuth before forwarding the request.
 * 
 * Flow:
 * 1. External app redirects user to: GET {frontend_url}/api/oauth/authorize?client_id=...&redirect_uri=...&state=...
 * 2. This proxy checks if user has a valid NextAuth session
 * 3. If not authenticated, redirect to login with returnUrl
 * 4. If authenticated, forward request to C# backend with Authorization header
 * 5. C# backend validates token, generates auth code, and redirects to external app
 */
export async function GET(request: NextRequest) {
  try {
    // Check if user is authenticated via NextAuth
    const session = await auth();
    
    if (!session || !session.tokens?.access_token) {
      // User not authenticated - redirect to login page
      // Preserve all query parameters in the returnUrl
      const returnUrl = `/api/oauth/authorize${request.nextUrl.search}`;
      const loginUrl = new URL('/login/signin', request.url);
      loginUrl.searchParams.set('returnUrl', returnUrl);
      
      console.log(`User not authenticated, redirecting to login: ${loginUrl.toString()}`);
      return NextResponse.redirect(loginUrl);
    }

    // User is authenticated - forward request to C# backend
    const backendUrl = process.env.BACKEND_URL || "http://localhost:5095";
    const targetUrl = `${backendUrl}/api/oauth/authorize${request.nextUrl.search}`;
    
    console.log(`Forwarding authenticated request to C# backend: ${targetUrl}`);
    
    // Make request to C# backend with user's Okta access token
    const backendResponse = await fetch(targetUrl, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${session.tokens.access_token}`,
        'Content-Type': 'application/json',
      },
      redirect: 'manual' // Don't automatically follow redirects
    });

    // Handle redirect responses from C# backend
    if (backendResponse.status >= 300 && backendResponse.status < 400) {
      const location = backendResponse.headers.get('Location');
      if (location) {
        console.log(`C# backend returned redirect to: ${location}`);
        return NextResponse.redirect(location);
      }
    }

    // Handle error responses
    if (!backendResponse.ok) {
      const errorBody = await backendResponse.text();
      console.error(`C# backend returned error: ${backendResponse.status} - ${errorBody}`);
      
      try {
        const errorJson = JSON.parse(errorBody);
        return NextResponse.json(errorJson, { status: backendResponse.status });
      } catch {
        return NextResponse.json(
          { error: "server_error", error_description: "Backend request failed" },
          { status: backendResponse.status }
        );
      }
    }

    // Return successful response
    const responseBody = await backendResponse.text();
    return new NextResponse(responseBody, {
      status: backendResponse.status,
      headers: {
        'Content-Type': backendResponse.headers.get('Content-Type') || 'application/json',
      },
    });
    
  } catch (error) {
    console.error("Error in OAuth authorize proxy:", error);
    return NextResponse.json(
      { 
        error: "server_error", 
        error_description: "An unexpected error occurred in the authorization proxy" 
      },
      { status: 500 }
    );
  }
}