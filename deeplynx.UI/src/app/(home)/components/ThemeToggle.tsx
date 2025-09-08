// app/components/ThemeToggle.tsx
"use client";
import { useEffect, useState } from "react";
import { MoonIcon, SunIcon } from "@heroicons/react/24/outline";

const THEME_KEY = "dlx-theme";

export default function ThemeToggle() {
  const [isDark, setIsDark] = useState(false);

  // Sync from <html data-theme> or localStorage on mount
  useEffect(() => {
    const getTheme = () =>
      document.documentElement.getAttribute("data-theme") ||
      localStorage.getItem(THEME_KEY) ||
      "light";
    setIsDark(getTheme() === "dark");

    // Keep in sync if theme changes elsewhere (another page/control)
    const onStorage = (e: StorageEvent) => {
      if (e.key === THEME_KEY && e.newValue) setIsDark(e.newValue === "dark");
    };
    window.addEventListener("storage", onStorage);

    // Also observe direct attribute changes (navigation, scripts)
    const obs = new MutationObserver(() => {
      const theme = document.documentElement.getAttribute("data-theme");
      if (theme) setIsDark(theme === "dark");
    });
    obs.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ["data-theme"],
    });

    return () => {
      window.removeEventListener("storage", onStorage);
      obs.disconnect();
    };
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const checked = e.target.checked;
    setIsDark(checked);

    // Update <html> and persist; matches your layout initializer logic
    const next = checked ? "dark" : "light";
    document.documentElement.setAttribute("data-theme", next);
    localStorage.setItem(THEME_KEY, next);

    // If you still want DaisyUI to react to "theme-controller", leave the class
    // (not required when controlling manually, but harmless)
  };

  return (
    <label className="toggle text-base-content">
      <input
        type="checkbox"
        value="dark"
        className="theme-controller"
        checked={isDark}
        onChange={handleChange}
      />
      <SunIcon className="size-4" />
      <MoonIcon className="size-4" />
    </label>
  );
}
