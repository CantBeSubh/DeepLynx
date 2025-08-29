// src/app/api/debug-callback/route.ts
import { NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
  const url = new URL(request.url);
  const code = url.searchParams.get('code');
  const state = url.searchParams.get('state');
  const error = url.searchParams.get('error');
  
  console.log("DEBUG CALLBACK - Code:", code);
  console.log("DEBUG CALLBACK - State:", state);
  console.log("DEBUG CALLBACK - Error:", error);
  
  return NextResponse.json({
    code: !!code,
    state: !!state,
    error,
    timestamp: new Date().toISOString(),
  });
}