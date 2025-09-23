"use client";

import React from "react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { peopleData } from "../dummy_data/data";
import AvatarCell from "../components/Avatar";
import { useLanguage } from "@/app/contexts/Language";
import ThemeToggle from "../components/ThemeToggle";
import ProjectSettings from "../components/ProjectSettings";

import {
  AdjustmentsHorizontalIcon,
  ArrowUpTrayIcon,
  BookmarkSquareIcon
} from "@heroicons/react/24/outline";

const ProjectSettingsClient = () => {
  const { t } = useLanguage(); // <-- t comes from translations[lang]
  const jason = peopleData.find((p) => p.name === "Jason");

    return (
        <div>
            <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
                <div>
                <h1 className="text-2xl font-bold text-info-content">
                    {t.translations.PROJECT_SETTINGS}
                </h1>
                </div>
            </div>

            <div className="flex w-full gap-8 p-8">
                <div className="w-full">
                    <ProjectSettings />
                </div>
            </div>
        </div>
    );
};

export default ProjectSettingsClient;