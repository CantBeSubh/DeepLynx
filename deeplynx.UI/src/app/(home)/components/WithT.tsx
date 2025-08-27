// app/components/WithT.tsx
"use client";
import { useLanguage } from "@/app/contexts/Language";
import { translations } from "@/app/lib/translations";

type Translation = (typeof translations)["en"];

export default function WithT({
  children,
}: {
  children: (t: Translation) => React.ReactNode;
}) {
  const { t } = useLanguage();
  return <>{children(t)}</>;
}
