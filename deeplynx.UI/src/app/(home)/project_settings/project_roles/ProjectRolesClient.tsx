"use client";

import React from "react";
import { useLanguage } from "@/app/contexts/Language";
// import ProjectSettings from "../components/ProjectSettingsTable/ProjectSettings";

const ProjectRolesClient = () => {
  const { t } = useLanguage();

    return (
        <div>
            <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
                <div>
                <h1 className="text-2xl font-bold text-info-content">
                    {t.translations.PROJECT_ROLES}
                </h1>
                </div>
            </div>

            <div className="flex w-full gap-8 p-8">
                <div className="w-full">
                </div>
            </div>
        </div>
    );
};

export default ProjectRolesClient;