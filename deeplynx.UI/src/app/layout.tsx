// src/app/layout.tsx (Server Component)
import "./globals.css";
import "react-loading-skeleton/dist/skeleton.css";
import "shepherd.js/dist/css/shepherd.css";
import "../../styles/shepherd-theme.css";
import ClientProviders from "./contexts/ClientProviders";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" data-theme="light" suppressHydrationWarning>
      <head>
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
    document.documentElement.setAttribute('data-theme', saved || 'light');
    
    document.addEventListener('change', function (e) {
      var t = e.target;
      if (t && t.classList && t.classList.contains('theme-controller')) {
        var theme = t.value;
        var checked = t.checked;
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
      <body className="min-h-screen bg-base-100 text-base-content">
        <ClientProviders>{children}</ClientProviders>
      </body>
    </html>
  );
}