// Create this file: src/app/test-env/page.tsx
"use client";

export default function TestEnvPage() {
  const authDisabled = process.env.NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION;
  const apiUrl = process.env.NEXT_PUBLIC_API_URL;

  return (
    <div className="min-h-screen bg-base-100 p-8">
      <h1 className="text-2xl font-bold mb-4">Environment Variable Test</h1>
      
      <div className="bg-base-200 p-4 rounded-lg space-y-2">
        <div>
          <strong>NEXT_PUBLIC_DISABLE_FRONTEND_AUTHENTICATION:</strong>
          <pre className="bg-base-300 p-2 mt-1 rounded">
            {JSON.stringify(authDisabled, null, 2)}
          </pre>
          <p className="text-sm mt-1">
            Type: {typeof authDisabled} | 
            Is "true"? {authDisabled === "true" ? "✅ YES" : "❌ NO"}
          </p>
        </div>

        <div>
          <strong>NEXT_PUBLIC_API_URL:</strong>
          <pre className="bg-base-300 p-2 mt-1 rounded">
            {JSON.stringify(apiUrl, null, 2)}
          </pre>
        </div>
      </div>
    </div>
  );
}