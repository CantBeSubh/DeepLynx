// app/layout.tsx
import "./globals.css"; // <— Tailwind/DaisyUI
import "react-loading-skeleton/dist/skeleton.css";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      {/* Apply your theme if you use daisyUI */}
      <body data-theme="light">{children}</body>
    </html>
  );
}
