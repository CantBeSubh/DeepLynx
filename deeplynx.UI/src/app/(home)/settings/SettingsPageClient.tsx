// src/app/(home)/settings/SettingsPageClient.tsx

"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useSession } from "next-auth/react";
import AvatarCell from "../components/Avatar";
import ThemeToggle from "../components/ThemeToggle";

const SettingsPageClient = () => {
  const { lang, setLang, t } = useLanguage();
  const { data: session } = useSession();

  const name = session?.user?.name ?? "";
  const email = session?.user?.email ?? "";
  const image = session?.user?.image ?? undefined;

  return (
    <div className="min-h-screen bg-base-100 p-8 lg:p-20">
      {/* User Header Section */}
      <div className="flex items-center mb-8">
        <AvatarCell image={image} name={name} size={20} />
        <h1 className="ml-3 text-2xl font-bold text-base-content">
          {session?.user?.name}
        </h1>
      </div>

      {/* Main Content Container */}
      <div className="lg:ml-20 max-w-4xl">
        {/* User Settings Section */}
        <section className="mb-12">
          <h2 className="text-lg font-bold text-base-content mb-6">
            {t.translations.USER_SETTINGS ?? "User Settings"}
          </h2>

          {/* General Information */}
          <div className="bg-base-200 rounded-box p-6">
            <h3 className="font-bold text-base-content mb-4">
              {t.translations.GENERAL ?? "General"}
            </h3>
            <div className="divider my-2" />

            <div className="space-y-3">
              <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                <span className="font-semibold text-base-content min-w-[140px]">
                  {t.translations.NAME ?? "Name"}:
                </span>
                <span className="text-base-content/80">{name}</span>
              </div>

              <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                <span className="font-semibold text-base-content min-w-[140px]">
                  {t.translations.EMAIL ?? "Email"}:
                </span>
                <span className="text-base-content/80">{email ?? "—"}</span>
              </div>

              <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                <span className="font-semibold text-base-content min-w-[140px]">
                  {t.translations.PROFILE_PICTURE ?? "Profile Picture"}:
                </span>
                <span className="text-base-content/80 truncate">{image}</span>
              </div>
            </div>
          </div>
        </section>

        {/* Preferences Section */}
        <section className="mb-12">
          <h2 className="text-lg font-bold text-base-content mb-6">
            {t.translations.PREFERENCES ?? "Preferences"}
          </h2>

          <div className="bg-base-200 rounded-box p-6">
            <div className="space-y-6">
              {/* Dark Mode Toggle */}
              <div className="flex items-center justify-between">
                <span className="font-semibold text-base-content">
                  {t.translations.DARK_MODE ?? "Dark Mode"}
                </span>
                <ThemeToggle />
              </div>

              <div className="divider my-2" />

              {/* Email Notifications */}
              <div className="flex items-center justify-between">
                <span className="font-semibold text-base-content">
                  {t.translations.EMAIL_NOTIFICATIONS ?? "Email Notifications"}
                </span>
                <input
                  type="checkbox"
                  defaultChecked
                  className="toggle toggle-primary"
                />
              </div>

              <div className="divider my-2" />

              {/* Language Selection */}
              <div className="flex items-center justify-between">
                <span className="font-semibold text-base-content">
                  {t.translations.LANGUAGE ?? "Language"}
                </span>
                <select
                  className="select select-primary select-sm"
                  value={lang}
                  onChange={(e) => setLang(e.target.value as "en" | "es")}
                >
                  <option value="en">English</option>
                  <option value="es">Español</option>
                </select>
              </div>
            </div>
          </div>
        </section>

        {/* API Keypairs Section */}
        <section>
          <h2 className="text-lg font-bold text-base-content mb-6">
            {t.translations.API_KEYPAIRS ?? "API Keypairs"}
          </h2>

          <div className="bg-base-200 rounded-box p-6">
            <p className="text-base-content/60">No API keypairs configured</p>
            {/* Add your API keypair content here */}
          </div>
        </section>
      </div>
    </div>
  );
};

export default SettingsPageClient;
