import { Suspense } from "react";
import AuthCallbackClient from "./AuthCallbackClient";

export default function Page() {
  return (
    <Suspense fallback={<p className="p-8 text-center">Logging you in...</p>}>
      <AuthCallbackClient />
    </Suspense>
  );
}
