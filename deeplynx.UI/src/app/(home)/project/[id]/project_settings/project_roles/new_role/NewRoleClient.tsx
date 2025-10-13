"use client";

import React, { useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { useRouter } from "next/navigation";
import AddRole from "../../../../../components/ProjectSettingsTable/ProjectTables/AddRole";
import { RoleResponseDto, PermissionResponseDto, ProjectResponseDto } from "../../../../../types/types";
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import { createRole } from "@/app/lib/role_services.client";


type NewRoleClientProps = {
    projectId: number;
    id?: string | null;
    name?: string | null;
    description?: string | null;
    lastUpdatedAt?: string | Date;
    lastUpdatedBy?: string | null;
    isArchieved?: boolean;
    organizationId?: number;
    roleId?: number;
    // refreshRoles: () => void;
};

export default function NewRoleClient({ projectId }: NewRoleClientProps) {
    const { t } = useLanguage();
    const router = useRouter();

    const [role, setRole] = useState<RoleResponseDto>({
        id: Date.now(),
        name: '',
        description: null,
        lastUpdatedAt: '',
        lastUpdatedBy: '',
        projectId: projectId,
        organizationId: undefined,
        roleId: undefined,
        isArchieved: false,
    });

    const [permissions, setPermissions] = useState<PermissionResponseDto[]>([]);

    // const handleSaveRole = async () => {
    //     if (!role.name) {
    //         console.error("Role name is required.");
    //         return;
    //     }

    //     try {
    //         const createdRole = await createRole(
    //             role.name,
    //             role.description || null,
    //             projectId
    //         );
    //         console.log("Role Created:", createdRole);
    //         await refreshRoles();
    //         router.push(`/project/${projectId}/project_settings?tab=Roles`);
    //     } catch (error) {
    //         console.error("Error creating role:", error);
    //     }
    // };

    const handleSaveRole = async () => {
        if (!role.name) {
            console.error("Role name is required.");
            return;
        }

        try {
            const createdRole = await createRole(
                role.name,
                role.description || null,
                projectId
            );
            console.log("Role Created:", createdRole);
            // await refreshRoles();
            router.push(`/project/${projectId}/project_settings?tab=Roles`);
        } catch (error) {
            console.error("Error creating role:", error);
        }
    };

    const handleCancel = () => {
        // Logic to handle cancellation
        console.log("Role creation canceled");
        //Logic to return to the roles page
        console.log("Navigating to roles with ID:", projectId);
        router.push(`/project/${projectId}/project_settings?tab=Roles`);
    };

    const handleReturnToRoles = () => {
        console.log("Navigating to roles with ID:", projectId);
        router.push(`/project/${projectId}/project_settings?tab=Roles`);
    };

    return (
        <div>
            <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
                <div>
                    <h1 className="text-2xl font-bold text-info-content">
                        {t.translations.ADD_NEW_ROLE}
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
                    <AddRole
                        role={role}
                        setRole={setRole}
                        permissions={permissions}
                        setPermissions={setPermissions}
                        onCancel={handleCancel}
                        onSave={handleSaveRole}
                    />
                </div>
            </div>
        </div>
    );
};