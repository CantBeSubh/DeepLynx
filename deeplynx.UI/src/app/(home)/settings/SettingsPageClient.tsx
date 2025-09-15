"use client";

import React from "react";
import { peopleData } from "../dummy_data/data";
import AvatarCell from "../components/Avatar";
import { MoonIcon, SunIcon } from "@heroicons/react/24/outline";
import { useLanguage } from "@/app/contexts/Language";
import ThemeToggle from "../components/ThemeToggle";

const SettingsPageClient = () => {
  const { lang, setLang, t } = useLanguage();
  const jason = peopleData.find((p) => p.name === "Jason");

  return (
    <div className="min-h-screen bg-base-100 p-8 lg:p-20">
      {/* User Header Section */}
      <div className="flex items-center mb-8">
        <AvatarCell image={jason?.image} name={jason?.name} size={20} />
        <h1 className="ml-3 text-2xl font-bold text-base-content">
          Jason Kuipers
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
                <span className="text-base-content/80">{jason?.name}</span>
              </div>

              <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                <span className="font-semibold text-base-content min-w-[140px]">
                  {t.translations.EMAIL ?? "Email"}:
                </span>
                <span className="text-base-content/80">
                  {jason?.email ?? "—"}
                </span>
              </div>

              <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                <span className="font-semibold text-base-content min-w-[140px]">
                  {t.translations.PROFILE_PICTURE ?? "Profile Picture"}:
                </span>
                <span className="text-base-content/80 truncate">
                  {jason?.image}
                </span>
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
