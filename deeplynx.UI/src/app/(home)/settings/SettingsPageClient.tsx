"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useSession } from "next-auth/react";
import AvatarCell from "../components/Avatar";
import ThemeToggle from "../components/ThemeToggle";

const SettingsPageClient = () => {
  const { lang, setLang, t } = useLanguage(); // <-- t comes from translations[lang]
  const { data: session } = useSession();

  const name = session?.user?.name ?? "";
  const email = session?.user?.email ?? "";
  const image = session?.user?.image ?? undefined;

  return (
    <div className="p-20">
      <div className="flex items-center">
        <AvatarCell image={image} name={name} size={20} />
        <h1 className="ml-3 text-2xl font-bold text-info-content">
          {session?.user?.name}
        </h1>
      </div>

      <div className="ml-20">
        <div>
          <span className="font-bold text-base-content text-black">
            {t.translations.USER_SETTINGS ?? "User Settings"}
          </span>
          <div className="mt-10">
            <span className="font-bold text-base-content text-black">
              {t.translations.GENERAL ?? "General"}
            </span>
            <div className="divider" />
            <p className="text-base-content">
              <span className="mr-5 font-bold text-black">
                {t.translations.NAME ?? "Name"}:
              </span>{" "}
              {name}
            </p>
            <p className="text-base-content">
              <span className="mr-5 font-bold text-black">
                {t.translations.EMAIL ?? "Email"}:
              </span>{" "}
              {email ?? "—"}
            </p>
            <p className="text-base-content">
              <span className="mr-5 font-bold text-black">
                {t.translations.PROFILE_PICTURE ?? "Profile Picture"}:
              </span>
              {session?.user?.image}
            </p>
          </div>
        </div>

        <div className="mt-20 gap-4">
          <span className="font-bold text-base-content text-black">
            {t.translations.PREFERENCES ?? "Preferences"}
          </span>
          <div className="divider" />

          <p className="flex items-center gap-4">
            <span className="font-bold text-base-content text-black">
              {t.translations.DARK_MODE ?? "Dark Mode"}
            </span>
            <ThemeToggle />
          </p>
          <p>
            <span className="font-bold text-black">
              {t.translations.EMAIL_NOTIFICATIONS}
            </span>{" "}
            <input type="checkbox" defaultChecked className="toggle ml-5" />
          </p>
          <p className="flex items-center gap-5">
            <span className="font-bold text-base-content text-black">
              {t.translations.LANGUAGE ?? "Language"}
            </span>
            <select
              className="select select-info select-sm mt-2"
              value={lang}
              onChange={(e) => setLang(e.target.value as "en" | "es")}
            >
              <option value="en">English</option>
              <option value="es">Español</option>
            </select>
          </p>
        </div>
        <div className="my-20">
          <span>API Keypairs</span> <div className="divider"></div>{" "}
        </div>
      </div>
    </div>
  );
};

export default SettingsPageClient;
