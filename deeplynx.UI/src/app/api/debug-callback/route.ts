// src/app/api/debug-callback/route.ts
import { NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
  const url = new URL(request.url);
  const code = url.searchParams.get('code');
  const state = url.searchParams.get('state');
  const error = url.searchParams.get('error');
  
  return NextResponse.json({
    code: !!code,
    state: !!state,
    error,
    timestamp: new Date().toISOString(),
  });
}