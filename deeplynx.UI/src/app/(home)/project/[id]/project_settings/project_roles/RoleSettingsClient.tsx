"use client";

import React, { useEffect, useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import RoleSettings from "../../../../components/ProjectSettingsTable/ProjectTables/RoleSettings";
import { useRouter } from "next/router";
import { useSearchParams } from "next/navigation";
import SettingsTab from "@/app/(home)/components/ProjectSettingsTable/ProjectTables/RoleName";
import PermissionsTab from "@/app/(home)/components/ProjectSettingsTable/ProjectTables/RolePermissions";
import Tabs from "@/app/(home)/components/Tabs";

type Props = {
    projectId: string | string[];
};

export default function RoleSettingsClient({ projectId }: Props) {
    const { t } = useLanguage();
    const router = useRouter();

    const handleReturnToRoles = () => {
        router.push(`/project/${projectId}/project_settings?tab=Roles`);
    };
    const [activeTab, setActiveTab] = useState("Settings");
    const searchParams = useSearchParams();

    const handleTabChange = (label: string) => {
        setActiveTab(label);
    };

    const toPermissionsTab = () => {
        setActiveTab("Permissions");
    };

    const onCancel = () => {
        router.push(`/project_settings?tab=Roles`);
    };

    const onSave = () => {
        // TODO Add logic to save role changes
        router.push(`/project_settings?tab=Roles`);
    };

    // Effect to read roleId from query and perform any necessary logic
    useEffect(() => {
        const roleId = searchParams.get('roleId');
        if (roleId) {
            // Fetch the role data using the roleId if needed
        }
    }, [searchParams]);

    const tabData = [
        { label: "Settings", content: <SettingsTab toPermissionsTab={toPermissionsTab} onCancel={onCancel} /> },
        { label: "Permissions", content: <PermissionsTab onCancel={onCancel} onSave={onSave} /> },
    ];

    return (
        <div>
            <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
                <div>
                    <h1 className="text-2xl font-bold text-info-content">
                        {t.translations.ROLE_SETTINGS}
                    </h1>
                    <div className="flex justify-start items-center">
                        <button
                            className="flex items-center justify-start space-x-2"
                            onClick={handleReturnToRoles}>
                                <ArrowLeftIcon className="size-4 text-secondary"/>
                            <span>{t.translations.RETURN_TO_ROLES}</span>
                        </button>
                    </div>
                </div>
            </div>

            <div className="flex w-full gap-8 p-8">
                <div className="w-full">
                    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
                        <div className="card-body">
                            <div className="flex justify-between items-start">
                                <h2 className="card-title">{t.translations.ROLE_SETTINGS}</h2>
                                {/* TODO Differentiate the name of the role instead of just role settings */}
                            </div>
                            <div className="w-full">
                                <Tabs
                                    tabs={tabData}
                                    className="tabs tabs-border"
                                    onTabChange={handleTabChange}
                                    activeTab={activeTab}
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};