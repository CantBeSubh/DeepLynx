// src/app/(home)/settings/SettingsPageClient.tsx
"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useSession } from "next-auth/react";
import AvatarCell from "../components/Avatar";
import ThemeToggle from "../components/ThemeToggle";
import { PlusIcon, TrashIcon, XMarkIcon } from "@heroicons/react/24/outline";
import toast from "react-hot-toast";
import {
  createApiKey,
  deleteApiKey,
  getAllKeysByUser,
} from "@/app/lib/settings_services.client";
import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import UserSettingsSkeleton from "../components/skeletons/usersettingsskeleton";
import ToastInfoModal from "../components/ToastInfoModal";
const SettingsPageClient = () => {
  const { lang, setLang, t } = useLanguage();
  const { data: session } = useSession();
  const [userKeys, setUserKeys] = useState<string[]>([]);
  const name = session?.user?.name ?? "";
  const email = session?.user?.email ?? "";
  const image = session?.user?.image ?? undefined;
  const [creating, setCreating] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  const apiKeyGen = async () => {
    try {
      setCreating(true);
      const res = await createApiKey();
      const keyStr = "Key: " + res.apiKey;
      const secretStr = "Secret: " + res.apiSecret;
      setUserKeys((prev) => [res.apiKey, ...prev]);
      toast.success(
        (t) => (
          <ToastInfoModal
            title={
              "API Keypair created successfully! Keep these somewhere safe:"
            }
            toastId={t.id}
            infoDisplay={[keyStr, secretStr]}
          />
          // <div className="relative">
          //   <div className="space-y-1 pb-5">
          //     <div className="flex justify-center w-full">
          //       <span>API Keypair created successfully! Keep these somewhere safe:</span>
          //     </div>
          //     <code className="block">Key: {res.apiKey}</code>
          //     <code className="block">Secret: {res.apiSecret}</code>
          //   </div>
          //   <div className="flex justify-center w-full">
          //     <button className="btn btn-primary btn-outline btn-xs" onClick={()=>{toast.dismiss(t.id); router.refresh()}}>
          //     Dismiss
          //     </button>
          //   </div>

          // </div>
        ),
        {
          duration: Infinity,
          style: {
            maxWidth: "none",
          },
        }
      );
    } catch (error) {
      console.error("Error creating keypair:", error);
      toast.error("API Keypair creation failed.");
    } finally {
      setCreating(false);
    }
  };

  const apiKeyDelete = (key: string) => async (e: React.MouseEvent) => {
    try {
      setDeleting(true);
      const res = await deleteApiKey(key);
      setUserKeys((prev) => prev.filter((x) => x !== key));
      toast.success("API Keypair deleted successfully!");
    } catch (error) {
      console.error("Error deleteing keypair:", error);
      toast.error("API Keypair deletion failed.");
    } finally {
      setDeleting(false);
    }
  };

  useEffect(() => {
    (async () => {
      try {
        const keys = await getAllKeysByUser();
        setUserKeys(keys);
        setIsLoading(false);
      } catch (error) {
        console.error(error);
      }
    })();
  }, []);

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
              <div className="flex items-center justify-between">
                <span className="font-semibold text-base-content">
                  {t.translations.DARK_MODE ?? "Dark Mode"}
                </span>
                <ThemeToggle />
              </div>

              <div className="divider my-2" />

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
          <div className="flex items-center justify-between w-full pt-5">
            <h2 className="w-50 text-lg font-bold text-base-content mb-6">
              {t.translations.API_KEYPAIRS ?? "API Keypairs"}
            </h2>

            {/* NOTE: don't call the function; pass it */}
            <button
              type="button"
              className="btn btn-sm btn-outline btn-primary"
              onClick={apiKeyGen}
              disabled={creating}
            >
              <PlusIcon className="size-5" />
              <span>{creating ? "Generating…" : "Generate"}</span>
            </button>
          </div>

          {isLoading ? (
            <UserSettingsSkeleton />
          ) : (
            <div className="bg-base-200 rounded-box p-6">
              {userKeys.length > 0 ? (
                <ul>
                  {userKeys.map((k, i) => (
                    <li
                      key={k}
                      className="flex items-center font-mono text-sm pb-3"
                    >
                      {i + 1}: {k}
                      <button
                        className="ml-auto"
                        type="button"
                        onClick={apiKeyDelete(k)}
                        disabled={deleting}
                      >
                        <TrashIcon className="text-red-400 hover:text-red-700 hover:cursor-pointer size-5" />
                      </button>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="text-base-content/60">
                  No API keypairs configured
                </p>
              )}
            </div>
          )}
        </section>
      </div>
    </div>
  );
};

export default SettingsPageClient;
