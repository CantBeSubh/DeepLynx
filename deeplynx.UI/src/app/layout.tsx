// src/app/layout.tsx (Server Component)
import "./globals.css";
import "react-loading-skeleton/dist/skeleton.css";
import { LanguageProvider } from "./contexts/Language";
import { SessionProvider } from "next-auth/react";
import { Toaster } from "react-hot-toast";
import "shepherd.js/dist/css/shepherd.css";
import "../../styles/shepherd-theme.css";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    // Set a safe default to prevent a white flash; will be replaced by script
    <html lang="en" data-theme="light" suppressHydrationWarning>
      <head>
        {/* Theme initializer: runs before React hydrates */}
        <script
          dangerouslySetInnerHTML={{
            __html: `
(function () {
  try {
    var KEY = 'dlx-theme';
    var saved = localStorage.getItem(KEY);
    if (saved) {
      document.documentElement.setAttribute('data-theme', saved);
    }
    var saved = localStorage.getItem(KEY);
    document.documentElement.setAttribute('data-theme', saved || 'light');
    
    // Persist changes from any DaisyUI theme controller (checkbox or radio)
    document.addEventListener('change', function (e) {
      var t = e.target;
      if (t && t.classList && t.classList.contains('theme-controller')) {
        var theme = t.value;
        var checked = t.checked;
        // For checkbox pattern: checked -> theme, unchecked -> light
        var next = checked ? theme : 'light';
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem(KEY, next);
      }
    }, { capture: true });
  } catch (e) {}
})();
          `,
          }}
        />
      </head>

      {/* Use DaisyUI tokens so colors switch with the theme */}
      <body className="min-h-screen bg-base-100 text-base-content">
        <SessionProvider>
          <LanguageProvider>
            {children}
            <Toaster />
          </LanguageProvider>
        </SessionProvider>
      </body>
    </html>
  );
}


