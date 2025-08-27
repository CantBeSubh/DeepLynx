// app/components/WithT.tsx
"use client";
import { useLanguage } from "@/app/contexts/Language";

export default function WithT({
  children,
}: {
  children: (t: any) => React.ReactNode;
}) {
  const { t } = useLanguage();
  return <>{children(t)}</>;
}
