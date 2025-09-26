"use client";

import React from "react";
import { peopleData } from "../dummy_data/data";
import { useLanguage } from "@/app/contexts/Language";
import MemberSettings from "@/app/(home)/components/MemberSettingsTable/MemberSettings";

const MemberManagementClient = () => {
  const { lang, setLang, t } = useLanguage();
  const jason = peopleData.find((p) => p.name === "Jason");

  return (
    <div>
            <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
                <div>
                <h1 className="text-2xl font-bold text-info-content">
                    {t.translations.MEMBER_SETTINGS}
                </h1>
                </div>
            </div>

            <div className="flex w-full gap-8 p-8">
                <div className="w-full">
                    <MemberSettings />
                </div>
            </div>
        </div>
  );
};

export default MemberManagementClient;