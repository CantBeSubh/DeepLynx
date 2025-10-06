"use client";

import React from "react";
import { useLanguage } from "@/app/contexts/Language";
import ProjectSettings from "../../../components/ProjectSettingsTable/ProjectSettings";

type Props = {
    initialProjects: { id: string; name: string }[];
    initialSelectedProjects: string[];
};

export default function ProjectSettingsClient({
    initialProjects,
    initialSelectedProjects
}: Props) {
    const { t } = useLanguage();

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
                    {/* <ProjectSettings
                        initialProjects={initialProjects}
                        initialSelectedProjects={initialSelectedProjects}
                    /> */}
                </div>
            </div>
        </div>
    );
};