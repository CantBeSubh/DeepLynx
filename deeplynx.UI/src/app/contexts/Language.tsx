"use client";
import React, { createContext, useContext, useEffect, useState } from "react";
import { translations } from "@/app/lib/translations";

type Lang = "en" | "es";
type Ctx = {
  lang: Lang;
  setLang: (l: Lang) => void;
  t: (typeof translations)[Lang];
};

const LanguageCtx = createContext<Ctx | null>(null);

export function LanguageProvider({ children }: { children: React.ReactNode }) {
  const [lang, setLang] = useState<Lang>("en");

  // (optional) persist across reloads
  useEffect(() => {
    const saved = localStorage.getItem("lang") as Lang | null;
    if (saved === "en" || saved === "es") setLang(saved);
  }, []);
  useEffect(() => {
    localStorage.setItem("lang", lang);
  }, [lang]);

  return (
    <LanguageCtx.Provider value={{ lang, setLang, t: translations[lang] }}>
      {children}
    </LanguageCtx.Provider>
  );
}

export function useLanguage() {
  const ctx = useContext(LanguageCtx);
  if (!ctx) throw new Error("useLanguage must be used within LanguageProvider");
  return ctx;
}
